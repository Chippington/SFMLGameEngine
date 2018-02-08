﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace SFMLEngine.Services {
	public class ServiceManager : IUpdatable, IRenderable {
		private Dictionary<Type, IGameService> serviceMap;
		private List<IGameService> serviceList;

		public ServiceManager() {
			serviceMap = new Dictionary<Type, IGameService>();
			serviceList = new List<IGameService>();
		}

		public void onInitialize(GameContext context) {

		}

		public void registerService<T>() where T : IGameService {
			var inst = (IGameService)Activator.CreateInstance<T>();
			serviceMap.Add(typeof(T), inst);
			serviceList.Add(inst);
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

	}
}