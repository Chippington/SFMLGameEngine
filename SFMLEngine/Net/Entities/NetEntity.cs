using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetUtils.Net.Data;
using NetUtils.Net.Interfaces;
using NetUtils.Net.Services.Entities;
using NetUtils.Utilities;
using SFMLEngine.Entities;
using SFMLEngine.Entities.Components;
using NetUtils.Net.Default;

namespace SFMLEngine.Net.Entities {
	public class NetEntity : ObjectBase, SFMLEngine.Entities.IEntity, NetUtils.Net.Services.Entities.INetEntity, NetUtils.Net.Interfaces.IPacketHandler {
		public NetEntity() {
			clientFuncMap = new Dictionary<Type, List<Delegate>>();
			serverFuncMap = new Dictionary<Type, List<Delegate>>();

			clientSendQueue = new Queue<PacketInfo>();
			serverSendQueue = new Queue<PacketInfo>();

			addPacketCallback<P_PacketGroup>(cb_packetGroup);
		}

		/*-------------------------------------------------------------------------------------------*/
		/*-------------------------------------------------------------------------------------------*/
		/*-------------------------------------------------------------------------------------------*/

		#region SFML ENGINE IENTITY
		public ComponentSet components { get => throw new NotImplementedException(); set => throw new NotImplementedException(); }

		public ComponentSet getComponents() {
			throw new NotImplementedException();
		}

		public void onDispose(GameContext context) {
			throw new NotImplementedException();
		}

		public void onDraw(GameContext context) {
			throw new NotImplementedException();
		}

		public void onInitialize(GameContext context) {
			throw new NotImplementedException();
		}

		public void onUpdate(GameContext context) {
			throw new NotImplementedException();
		}

		public void setOwner(Scene owner) {
			throw new NotImplementedException();
		}
		#endregion

		/*-------------------------------------------------------------------------------------------*/
		/*-------------------------------------------------------------------------------------------*/
		/*-------------------------------------------------------------------------------------------*/

		#region NETUTILS INETENTITY

		/*-------------------------------------------------------------------------------------------*/
		/*-------------------------------------------------------------------------------------------*/
		/*-------------------------------------------------------------------------------------------*/

		#region Fields
		/// <summary>
		/// ID of this entity.
		/// </summary>
		internal ID _entityID;

		/// <summary>
		/// Entity service that owns this entity.
		/// </summary>
		internal NetEntityService _service;

		/// <summary>
		/// Flag that indicates whether or not this entity is owned by a server instance.
		/// </summary>
		internal bool _isServer;

		/// <summary>
		/// Flag that indicates whether or not this entity is owned by a client instance.
		/// </summary>
		internal bool _isClient;

		/// <summary>
		/// Unique GUID identifier for this entity.
		/// </summary>
		internal string _guid;

		/// <summary>
		/// Dictionary used to compare previous values of fields, to determine when to sync.
		/// </summary>
		private Dictionary<int, object> fieldValueMap;

		/// <summary>
		/// Ordered (by name) list of fields that must be synchronized.
		/// </summary>
		private FieldSet[] fieldList;

		/// <summary>
		/// Length of the header in bytes.
		/// </summary>
		private byte headerByteCount {
			get {
				return (byte)((fieldList.Length / 8) + 1);
			}
		}

		public ID entityID { get => _entityID; set => _entityID = value; }
		public NetEntityService service { get => _service; set => _service = value; }
		public bool isServer { get => _isServer; set => _isServer = value; }
		public bool isClient { get => _isClient; set => _isClient = value; }
		public string guid { get => _guid; set => _guid = value; }
		#endregion

		/// <summary>
		/// Initializes the Entity. MUST be called, or synchronization map will not be built.
		/// </summary>
		public virtual void onNetInitialize() {
			buildSyncMap();
			fieldValueMap = new Dictionary<int, object>();
		}

		/// <summary>
		/// Update function called when the entity is updated (client only)
		/// </summary>
		/// <returns></returns>
		public virtual EntityState onClientUpdate() { return new EntityState(); }

		/// <summary>
		/// Update function called when the entity is updated (server only)
		/// </summary>
		/// <returns></returns>
		public virtual EntityState onServerUpdate() { return new EntityState(); }

		/// <summary>
		/// Builds the "synchronization" map, used to keep track of which fields need to be updated between server/client.
		/// </summary>
		public void buildSyncMap() {
			var props = GetType().GetFields()
				.Where(prop => prop.IsDefined(typeof(Synchronize), false))
				.OrderBy(prop => prop.Name)
				.ToList();

			fieldList = props.Select(field => new FieldSet() {
				attr = (Synchronize)field.GetCustomAttributes(true)
				.Where(i => i.GetType() == typeof(Synchronize))
				.FirstOrDefault(),
				field = field
			}).OrderBy((field) => field.field.Name).ToArray();

			for (int i = 0; i < fieldList.Length; i++)
				fieldList[i].headerIndex = i;
		}

		/// <summary>
		/// Returns a "changeset" containing a header and field data. The header contains bit flags that indicate which fields have updated.
		/// </summary>
		/// <returns></returns>
		public EntityFrame getChangeSet() {
			//Comparison function for determining which fields have changed.
			Func<FieldSet, bool> changedFunc = (field) => {
				int hash = getFieldHash(field);
				if (fieldValueMap.ContainsKey(hash) == false) return true;

				return (field.field.GetValue(null) != fieldValueMap[hash]);
			};

			//Create an array of fields that have been changed
			var changedFields = (from field in fieldList
								 where changedFunc(field)
								 select field).ToArray();

			//Create and initialize a byte array to 0 to use as the header
			byte[] header = new byte[headerByteCount];
			for (int i = 0; i < header.Length; i++)
				header[i] = 0;

			//Iterate through each field
			for (int i = 0; i < changedFields.Length; i++) {
				FieldSet field = changedFields[i];
				int byteIndex = field.headerIndex / 8;
				int bitIndex = field.headerIndex % 8;

				//Set this bit to true, since it has changed
				byte mask = (byte)(1 << bitIndex);
				header[byteIndex] |= mask;

				//Store the new value in the "previous value" map
				fieldValueMap[getFieldHash(field)] = field.field.GetValue(this);
			}

			//Create the frame representing these changes
			EntityFrame frame = new EntityFrame();
			var buff = frame.data = new DataBufferStream();

			//Write header to buffer
			for (int i = 0; i < header.Length; i++) {
				buff.write((byte)header[i]);
			}

			//Write fields to buffer using the dynamic type helper
			for (int i = 0; i < changedFields.Length; i++) {
				var field = changedFields[i].field;
				DynamicTypes.writeDynamic(buff, field.GetValue(this), field.FieldType);
			}

			return frame;
		}

		/// <summary>
		/// Applies a "changeset" containing a header and field data. The header contains bit flags that indicate which fields have updated.
		/// </summary>
		/// <param name="frame"></param>
		public void applyChangeSet(EntityFrame frame) {
			var buff = frame.data;

			//Read the header from the buffer
			byte[] header = new byte[headerByteCount];
			for (int i = 0; i < header.Length; i++)
				header[i] = buff.readByte();

			//Iterate through each field
			for (int i = 0; i < fieldList.Length; i++) {
				int byteIndex = i / 8;
				int bitIndex = i % 8;

				//If the bit flag is set, then the field has changed
				byte mask = (byte)(1 << bitIndex);
				bool isSet = (header[byteIndex] & mask) != 0;

				if (isSet == false) continue;

				//If the field has changed, then read/set it locally using the dynamic type helper
				FieldSet field = fieldList[i];
				var newVal = DynamicTypes.readDynamic(buff, field.field.FieldType);
				field.field.SetValue(this, newVal);
			}
		}

		/// <summary>
		/// Returns a "keyset" frame, containing all [Synchronize] marked fields.
		/// </summary>
		/// <returns></returns>
		public EntityFrame getKeySet() {
			//Since the fieldList is already ordered, no need to worry about ordering of data
			var fields = fieldList.ToArray();
			for (int i = 0; i < fields.Length; i++) {
				FieldSet field = fields[i];
				fieldValueMap[getFieldHash(field)] = field.field.GetValue(this);
			}

			EntityFrame frame = new EntityFrame();
			var buff = frame.data = new DataBufferStream();

			//Write all fields to the frame buffer using the dynamic type helper
			for (int i = 0; i < fields.Length; i++) {
				var field = fields[i].field;
				DynamicTypes.writeDynamic(buff, field.GetValue(this), field.FieldType);
			}

			return frame;
		}

		/// <summary>
		/// Applies a "keyset" frame, containing all [Synchronize] marked fields.
		/// </summary>
		/// <param name="frame"></param>
		public void applyKeySet(EntityFrame frame) {
			var buff = frame.data;

			//Read all fields from the frame buffer using the dynamic type helper
			for (int i = 0; i < fieldList.Length; i++) {
				FieldSet field = fieldList[i];
				var newVal = DynamicTypes.readDynamic(buff, field.field.FieldType);
				field.field.SetValue(this, newVal);
			}
		}

		/// <summary>
		/// Writes the creation data of the entity, as well as a keyset to the buffer.
		/// </summary>
		/// <param name="buffer"></param>
		public void writeTo(IDataBuffer buffer) {
			writeCreationData(buffer);

			var keyset = getKeySet();
			buffer.write((IDataBuffer)keyset.data);

			buffer.write((string)_guid);
		}

		/// <summary>
		/// Reads the creation data of the entity, as well as a keyset from the buffer.
		/// </summary>
		/// <param name="buffer"></param>
		public void readFrom(IDataBuffer buffer) {
			readCreationData(buffer);

			var keysetBuffer = buffer.readBuffer();
			var keyset = new EntityFrame() {
				data = keysetBuffer
			};

			applyKeySet(keyset);

			_guid = buffer.readString();
		}

		/// <summary>
		/// Writes any custom creation data to the buffer. Must correspond with the readCreationData() method.
		/// </summary>
		/// <param name="buffer"></param>
		public virtual void writeCreationData(IDataBuffer buffer) {

		}

		/// <summary>
		/// Reads any custom creation data from the buffer. Must correspond with the writeCreationData() method.
		/// </summary>
		/// <param name="buffer"></param>
		public virtual void readCreationData(IDataBuffer buffer) {

		}

		/// <summary>
		/// Returns a unique hash based on the field handle.
		/// </summary>
		/// <param name="field"></param>
		/// <returns></returns>
		private int getFieldHash(FieldSet field) {
			return field.field.FieldHandle.Value.ToInt32();
		}

		/// <summary>
		/// Sends a packet to the server. Routing is handled by the Entity service.
		/// </summary>
		/// <param name="info"></param>
		public void sendPacketToServer(PacketInfo<EntityPacket> info) {
			queueClientSendToServer(info);
		}

		/// <summary>
		/// Sends the packet to the client(s). Routing is handled by the Entity service.
		/// </summary>
		/// <param name="info"></param>
		public void sendPacketToClient(PacketInfo<EntityPacket> info) {
			queueServerSendToClients(info);
		}

		/// <summary>
		/// Returns the entity service that owns this entity.
		/// </summary>
		/// <returns></returns>
		public NetEntityService getService() {
			return _service;
		}

		/// <summary>
		/// Returns the ID of this entity.
		/// </summary>
		/// <returns></returns>
		public ID getEntityID() {
			return _entityID;
		}
		#endregion

		/*-------------------------------------------------------------------------------------------*/
		/*-------------------------------------------------------------------------------------------*/
		/*-------------------------------------------------------------------------------------------*/

		#region NETUTILS PACKET HANDLER
		private Dictionary<Type, List<Delegate>> clientFuncMap;
		private Dictionary<Type, List<Delegate>> serverFuncMap;

		private Queue<PacketInfo> clientSendQueue;
		private Queue<PacketInfo> serverSendQueue;

		private void cb_packetGroup(P_PacketGroup obj) {
			foreach (var packet in obj.packets)
				tryInvoke(packet);
		}

		/// <summary>
		/// Adds a packet callback for both clientside and serverside.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="func"></param>
		public void addPacketCallback<T>(Action<T> func) where T : Packet {
			addClientPacketCallback<T>(func);
			addServerPacketCallback<T>(func);
		}

		/// <summary>
		/// Adds a packet callback specific to the clientside.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="func"></param>
		public void addClientPacketCallback<T>(Action<T> func) where T : Packet {
			if (clientFuncMap.ContainsKey(typeof(T)) == false)
				clientFuncMap.Add(typeof(T), new List<Delegate>());

			clientFuncMap[typeof(T)].Add(func);
		}

		/// <summary>
		/// Adds a packet callback specific to the serverside.
		/// </summary>
		/// <param name="type"></param>
		/// <param name="func"></param>
		public void addServerPacketCallback<T>(Action<T> func) where T : Packet {
			if (serverFuncMap.ContainsKey(typeof(T)) == false)
				serverFuncMap.Add(typeof(T), new List<Delegate>());

			serverFuncMap[typeof(T)].Add(func);
		}

		/// <summary>
		/// Removes the callback function from the client mapping.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="func"></param>
		public void removeClientPacketCallback<T>(Action<T> func) where T : Packet {
			if (clientFuncMap.ContainsKey(typeof(T)) == true)
				clientFuncMap[typeof(T)].Remove(func);
		}

		/// <summary>
		/// Removes the callback function from the server mapping.
		/// </summary>
		/// <typeparam name="T"></typeparam>
		/// <param name="func"></param>
		public void removeServerPacketCallback<T>(Action<T> func) where T : Packet {
			if (serverFuncMap.ContainsKey(typeof(T)) == true)
				serverFuncMap[typeof(T)].Remove(func);
		}

		/// <summary>
		/// Called once the client receives a packet.
		/// Sorts the packet to the correct function callback.
		/// </summary>
		/// <param name="pack"></param>
		public void onClientReceivePacket(Packet pack) {
			var type = evaluateType(pack);
			if (clientFuncMap.ContainsKey(type)) {
				var funcList = clientFuncMap[type];
				if (funcList != null && funcList.Count > 0)
					for (int i = funcList.Count - 1; i >= 0; i--)
						if (funcList[i] != null) {
							var func = funcList[i];
							func.DynamicInvoke(pack);
						} else
							funcList.RemoveAt(i);
			} else {
				log(string.Format("[CL] Missing packet hook: {0}->{1}", this.GetType(), type));
			}
		}

		/// <summary>
		/// Called once the server receives a packet.
		/// Sorts the packet to the correct function callback.
		/// </summary>
		/// <param name="pack"></param>
		public void onServerReceivePacket(Packet pack) {
			var type = evaluateType(pack);
			if (serverFuncMap.ContainsKey(type)) {
				var funcList = serverFuncMap[type];
				if (funcList != null && funcList.Count > 0)
					for (int i = funcList.Count - 1; i >= 0; i--)
						if (funcList[i] != null)
							funcList[i].DynamicInvoke(pack);
						else
							funcList.RemoveAt(i);
			} else {
				log(string.Format("[SV] Missing packet hook: {0}->{1}", this.GetType(), type));
			}
		}

		/// <summary>
		/// Attempts to invoke the packet callback on both server and client.
		/// </summary>
		/// <param name="pack"></param>
		public void tryInvoke(Packet pack) {
			var type = evaluateType(pack);
			if (serverFuncMap.ContainsKey(type)) {
				var funcList = serverFuncMap[type];
				if (funcList != null && funcList.Count > 0)
					for (int i = funcList.Count - 1; i >= 0; i--)
						if (funcList[i] != null)
							funcList[i].DynamicInvoke(pack);
						else
							funcList.RemoveAt(i);
			}

			if (clientFuncMap.ContainsKey(type)) {
				var funcList = clientFuncMap[type];
				if (funcList != null && funcList.Count > 0)
					for (int i = funcList.Count - 1; i >= 0; i--)
						if (funcList[i] != null) {
							var func = funcList[i];
							func.DynamicInvoke(pack);
						} else
							funcList.RemoveAt(i);
			}
		}

		/// <summary>
		/// Evaluate the type of the given packet.
		/// </summary>
		/// <param name="pack"></param>
		/// <returns></returns>
		protected virtual Type evaluateType(Packet pack) {
			return pack.GetType();
		}

		/// <summary>
		/// Queues a packet for sending by the client that owns this packet handler.
		/// </summary>
		/// <param name="pack"></param>
		/// <param name="info"></param>
		public void queueClientSendToServer(PacketInfo info) {
			clientSendQueue.Enqueue(info);
		}

		/// <summary>
		/// Queues a packet for sending by the server that owns this packet handler.
		/// </summary>
		/// <param name="pack"></param>
		/// <param name="info"></param>
		public void queueServerSendToClients(PacketInfo info) {
			serverSendQueue.Enqueue(info);
		}

		/// <summary>
		/// Returns the next packet to send, if any.
		/// </summary>
		/// <returns></returns>
		public PacketInfo getNextOutgoingClientPacket() {
			if (clientSendQueue.Count == 0)
				return null;

			return clientSendQueue.Dequeue();
		}

		/// <summary>
		/// Returns the next packet to send, if any.
		/// </summary>
		/// <returns></returns>
		public PacketInfo getNextOutgoingServerPacket() {
			if (serverSendQueue.Count == 0)
				return null;

			return serverSendQueue.Dequeue();
		}
		#endregion
	}
}
