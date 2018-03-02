using NetUtils;
using SFMLEngine.Entities;
using SFMLEngine.Scenes;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Network {
	public interface INetScene : IScene, INetUpdatable, ISerializable {
	}
}
