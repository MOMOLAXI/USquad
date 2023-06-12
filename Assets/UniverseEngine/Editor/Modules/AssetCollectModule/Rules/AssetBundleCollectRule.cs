using System;
using System.Collections.Generic;
using System.Linq;

namespace UniverseEngine.Editor
{
	public abstract class AssetBundleCollectRule
	{
		public abstract void Init();
		public abstract List<RuleDisplayName> GetDisplayRuleNames();
		public abstract bool HasRuleName(string ruleName);

		public abstract T GetRuleInstance<T>(string ruleName) where T : class;

		protected static string GetRuleDisplayName(string name, Type type)
		{
			DisplayNameAttribute attribute = ReflectionUtilities.GetAttribute<DisplayNameAttribute>(type);
			if (attribute != null && !string.IsNullOrEmpty(attribute.DisplayName))
			{
				return attribute.DisplayName;
			}

			return name;
		}
	}

	public class AssetBundleCollectRule<T> : AssetBundleCollectRule where T : class
	{
		readonly Dictionary<string, Type> m_RuleType = new();
		readonly Dictionary<string, T> m_RuleInstance = new();

		public override void Init()
		{
			m_RuleType.Clear();
			m_RuleInstance.Clear();
			using (Collections.AllocAutoList(out List<Type> ruleTypes))
			{
				ReflectionUtilities.GetTypesDerivedFrom<T>(ruleTypes);
				for (int i = 0; i < ruleTypes.Count; i++)
				{
					Type type = ruleTypes[i];
					m_RuleType[type.Name] = type;
					m_RuleInstance[type.Name] = Activator.CreateInstance(type) as T;
				}
			}
		}

		public override List<RuleDisplayName> GetDisplayRuleNames()
		{
			return m_RuleType.Select(pair => new RuleDisplayName
			                 {
				                 ClassName = pair.Key,
				                 DisplayName = GetRuleDisplayName(pair.Key, pair.Value)
			                 })
			                 .ToList();
		}

		public override bool HasRuleName(string ruleName)
		{
			return m_RuleType.ContainsKey(ruleName);
		}

		public override TRule GetRuleInstance<TRule>(string ruleName)
		{
			return GetRuleInstance(ruleName) as TRule;
		}

		T GetRuleInstance(string ruleName)
		{
			if (m_RuleInstance.TryGetValue(ruleName, out T instance))
				return instance;

			// 如果不存在创建类的实例
			if (m_RuleType.TryGetValue(ruleName, out Type type))
			{
				instance = (T)Activator.CreateInstance(type);
				m_RuleInstance[ruleName] = instance;
				return instance;
			}

			throw new($"收集规则 : {typeof(T)} 类型无效：{ruleName}");
		}
	}
}
