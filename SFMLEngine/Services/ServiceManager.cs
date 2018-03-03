using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Services {
	public class ServiceManager : ObjectBase, IUpdatable, IRenderable {
		private Dictionary<Type, IGameService> serviceMap;
		private List<IGameService> serviceList;
		private GameContext context;

		public ServiceManager() {
			serviceMap = new Dictionary<Type, IGameService>();
			serviceList = new List<IGameService>();
		}

		public void onInitialize(GameContext context) {
			this.context = context;
		}

		public T registerService<T>() where T : IGameService {
			if (context.locked)
				throw new Exception("Cannot register a service after logic has been initialized.");

			log("Registering service of type " + typeof(T).Name);
			var inst = (T)Activator.CreateInstance<T>();
			serviceMap.Add(typeof(T), inst);
			var interfaces = typeof(T).GetInterfaces();
			foreach (var i in interfaces)
				if (i.GetInterfaces().Contains(typeof(IGameService)))
					serviceMap.Add(i, inst);

			serviceList.Add(inst);
			inst.onInitialize(context);

			return inst;
		}

		public T getService<T>() where T : IGameService {
			if (serviceMap.ContainsKey(typeof(T)) == false)
				return default(T);

			return (T)serviceMap[typeof(T)];
		}

		public void onDraw(GameContext context) {
			for (int i = 0; i < serviceList.Count; i++)
				serviceList[i].onDraw(context);
		}

		public void onUpdate(GameContext context) {
			for (int i = 0; i < serviceList.Count; i++)
				serviceList[i].onUpdate(context);
		}

		public void onDispose(GameContext context) {
			for (int i = 0; i < serviceList.Count; i++)
				serviceList[i].onDispose(context);
		}

		public bool hasService<T>() where T : IGameService {
			return serviceMap.ContainsKey(typeof(T));
		}
	}
}
