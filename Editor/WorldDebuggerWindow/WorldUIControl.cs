using System.Collections.Generic;
using UnityEditor.UIElements;
using UnityEngine;
using UnityEngine.UIElements;

namespace KVD.ECS.Editor.WorldDebuggerWindow
{
	public class WorldUIControl : BindableElement
	{
		public new class UxmlFactory : UxmlFactory<WorldUIControl, UxmlTraits>
		{
		}

		public new class UxmlTraits : VisualElement.UxmlTraits
		{
			UxmlStringAttributeDescription m_String =
				new UxmlStringAttributeDescription { name = "string-attr", defaultValue = "default_value" };
			UxmlFloatAttributeDescription m_Float =
				new UxmlFloatAttributeDescription { name = "float-attr", defaultValue = 0.1f };
			UxmlDoubleAttributeDescription m_Double =
				new UxmlDoubleAttributeDescription { name = "double-attr", defaultValue = 0.1 };
			UxmlIntAttributeDescription m_Int = new UxmlIntAttributeDescription { name = "int-attr", defaultValue = 2 };
			UxmlLongAttributeDescription m_Long =
				new UxmlLongAttributeDescription { name = "long-attr", defaultValue = 3 };
			UxmlBoolAttributeDescription m_Bool =
				new UxmlBoolAttributeDescription { name = "bool-attr", defaultValue = false };
			UxmlColorAttributeDescription m_Color =
				new UxmlColorAttributeDescription { name = "color-attr", defaultValue = Color.red };

			public override IEnumerable<UxmlChildElementDescription> uxmlChildElementsDescription
			{
				get
				{
					yield break;
				}
			}

			public override void Init(VisualElement ve, IUxmlAttributes bag, CreationContext cc)
			{
				base.Init(ve, bag, cc);
				var ate = (WorldUIControl)ve;

				ate.Clear();

				ate.stringAttr = m_String.GetValueFromBag(bag, cc);
				ate.Add(new TextField("String") { value = ate.stringAttr });

				ate.floatAttr = m_Float.GetValueFromBag(bag, cc);
				ate.Add(new FloatField("Float") { value = ate.floatAttr });

				ate.doubleAttr = m_Double.GetValueFromBag(bag, cc);
				ate.Add(new DoubleField("Double") { value = ate.doubleAttr });

				ate.intAttr = m_Int.GetValueFromBag(bag, cc);
				ate.Add(new IntegerField("Integer") { value = ate.intAttr });

				ate.longAttr = m_Long.GetValueFromBag(bag, cc);
				ate.Add(new LongField("Long") { value = ate.longAttr });

				ate.boolAttr = m_Bool.GetValueFromBag(bag, cc);
				ate.Add(new Toggle("Toggle") { value = ate.boolAttr });

				ate.colorAttr = m_Color.GetValueFromBag(bag, cc);
				ate.Add(new ColorField("Color") { value = ate.colorAttr });
			}
		}

		private string _stringAttr;

		public string stringAttr
		{
			get => _stringAttr;
			set
			{
				_stringAttr                       = value;
				this.Q<TextField>("String").value = _stringAttr;
			}
		}

		public float floatAttr{ get; set; }
		public double doubleAttr{ get; set; }
		public int intAttr{ get; set; }
		public long longAttr{ get; set; }
		public bool boolAttr{ get; set; }
		public Color colorAttr{ get; set; }
	}
}
