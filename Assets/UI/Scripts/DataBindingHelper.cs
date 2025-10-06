using UnityEngine;
using UnityEngine.UIElements;
using System;
using System.Collections.Generic;
using System.Linq;

public static class DataBindingHelper
{
    private static Dictionary<VisualElement, List<IBinding>> bindings = new Dictionary<VisualElement, List<IBinding>>();

    public interface IBinding
    {
        void Update();
        void Dispose();
    }

    private class PropertyBinding<T> : IBinding where T : IEquatable<T>
    {
        private readonly Func<T> getter;
        private readonly Action<T> setter;
        private T lastValue;

        public PropertyBinding(Func<T> getter, Action<T> setter)
        {
            this.getter = getter;
            this.setter = setter;
            this.lastValue = getter();
            setter(lastValue);
        }

        public void Update()
        {
            T currentValue = getter();
            if (!currentValue.Equals(lastValue))
            {
                lastValue = currentValue;
                setter(currentValue);
            }
        }

        public void Dispose()
        {
            // Nothing to dispose for simple property binding
        }
    }

    private class EventBinding : IBinding
    {
        private readonly Action unsubscribe;

        public EventBinding(Action unsubscribe)
        {
            this.unsubscribe = unsubscribe;
        }

        public void Update()
        {
            // Event bindings don't need updates
        }

        public void Dispose()
        {
            unsubscribe?.Invoke();
        }
    }

    private class ListBinding<T> : IBinding
    {
        private readonly Func<List<T>> getter;
        private readonly Action<List<T>> setter;
        private List<T> lastValue;

        public ListBinding(Func<List<T>> getter, Action<List<T>> setter)
        {
            this.getter = getter;
            this.setter = setter;
            this.lastValue = new List<T>(getter());
            setter(lastValue);
        }

        public void Update()
        {
            var currentValue = getter();
            if (!ListsEqual(currentValue, lastValue))
            {
                lastValue = new List<T>(currentValue);
                setter(lastValue);
            }
        }

        private bool ListsEqual(List<T> list1, List<T> list2)
        {
            if (list1 == null && list2 == null) return true;
            if (list1 == null || list2 == null) return false;
            if (list1.Count != list2.Count) return false;

            for (int i = 0; i < list1.Count; i++)
            {
                if (!EqualityComparer<T>.Default.Equals(list1[i], list2[i]))
                    return false;
            }
            return true;
        }

        public void Dispose()
        {
            // Nothing to dispose for list binding
        }
    }

    private class ObjectBinding<T> : IBinding
    {
        private readonly Func<T> getter;
        private readonly Action<T> setter;
        private T lastValue;

        public ObjectBinding(Func<T> getter, Action<T> setter)
        {
            this.getter = getter;
            this.setter = setter;
            this.lastValue = getter();
            setter(lastValue);
        }

        public void Update()
        {
            T currentValue = getter();
            if (!EqualityComparer<T>.Default.Equals(currentValue, lastValue))
            {
                lastValue = currentValue;
                setter(currentValue);
            }
        }

        public void Dispose()
        {
            // Nothing to dispose for object binding
        }
    }

    public static void BindText(VisualElement element, string elementName, Func<string> textGetter)
    {
        var label = element.Q<Label>(elementName);
        if (label != null)
        {
            var binding = new PropertyBinding<string>(textGetter, value => label.text = value);
            AddBinding(element, binding);
        }
    }

    public static void BindValue<T>(VisualElement element, string elementName, Func<T> valueGetter)
        where T : struct, IEquatable<T>
    {
        var valueElement = element.Q(elementName);
        if (valueElement is INotifyValueChanged<T> notifyElement)
        {
            var binding = new PropertyBinding<T>(valueGetter, value => notifyElement.SetValueWithoutNotify(value));
            AddBinding(element, binding);
        }
    }

    public static void BindValueObject<T>(VisualElement element, string elementName, Func<T> valueGetter)
    {
        var valueElement = element.Q(elementName);
        if (valueElement is INotifyValueChanged<T> notifyElement)
        {
            var binding = new ObjectBinding<T>(valueGetter, value => notifyElement.SetValueWithoutNotify(value));
            AddBinding(element, binding);
        }
    }

    public static void BindSlider(VisualElement element, string elementName, Func<float> valueGetter, Action<float> valueSetter = null)
    {
        var slider = element.Q<Slider>(elementName);
        if (slider != null)
        {
            // Bind value display
            var binding = new PropertyBinding<float>(valueGetter, value => slider.SetValueWithoutNotify(value));
            AddBinding(element, binding);

            // Bind value changes if setter provided
            if (valueSetter != null)
            {
                void OnValueChanged(ChangeEvent<float> evt) => valueSetter(evt.newValue);
                slider.RegisterValueChangedCallback(OnValueChanged);

                var eventBinding = new EventBinding(() => slider.UnregisterValueChangedCallback(OnValueChanged));
                AddBinding(element, eventBinding);
            }
        }
    }

    public static void BindButton(VisualElement element, string elementName, Action onClick)
    {
        var button = element.Q<Button>(elementName);
        if (button != null)
        {
            button.clicked += onClick;
            var binding = new EventBinding(() => button.clicked -= onClick);
            AddBinding(element, binding);
        }
    }

    public static void BindToggle(VisualElement element, string elementName, Func<bool> valueGetter, Action<bool> valueSetter = null)
    {
        var toggle = element.Q<Toggle>(elementName);
        if (toggle != null)
        {
            // Bind value display - bool implements IEquatable<bool> so this is safe
            var binding = new PropertyBinding<bool>(valueGetter, value => toggle.SetValueWithoutNotify(value));
            AddBinding(element, binding);

            // Bind value changes if setter provided
            if (valueSetter != null)
            {
                void OnValueChanged(ChangeEvent<bool> evt) => valueSetter(evt.newValue);
                toggle.RegisterValueChangedCallback(OnValueChanged);

                var eventBinding = new EventBinding(() => toggle.UnregisterValueChangedCallback(OnValueChanged));
                AddBinding(element, eventBinding);
            }
        }
    }

    public static void BindProgressBar(VisualElement element, string elementName, Func<float> valueGetter, Func<float> maxValueGetter = null)
    {
        var progressBar = element.Q<ProgressBar>(elementName);
        if (progressBar != null)
        {
            var binding = new PropertyBinding<float>(valueGetter, value => progressBar.value = value);
            AddBinding(element, binding);

            if (maxValueGetter != null)
            {
                var maxBinding = new PropertyBinding<float>(maxValueGetter, value => progressBar.highValue = value);
                AddBinding(element, maxBinding);
            }
        }
    }

    public static void BindDropdown<T>(VisualElement element, string elementName, Func<List<T>> optionsGetter, Func<T> valueGetter, Action<T> valueSetter = null)
    {
        var dropdown = element.Q<DropdownField>(elementName);
        if (dropdown != null)
        {
            // Create a custom binding for list options that compares content
            var optionsBinding = new ListBinding<T>(optionsGetter, options =>
            {
                dropdown.choices = options.ConvertAll(o => o.ToString());
            });
            AddBinding(element, optionsBinding);

            // Update selected value (use ObjectBinding for all types to avoid generic constraints)
            var valueBinding = new ObjectBinding<T>(valueGetter, value =>
            {
                dropdown.SetValueWithoutNotify(value.ToString());
            });
            AddBinding(element, valueBinding);

            // Handle value changes
            if (valueSetter != null)
            {
                void OnValueChanged(ChangeEvent<string> evt)
                {
                    var options = optionsGetter();
                    var selectedIndex = dropdown.choices.IndexOf(evt.newValue);
                    if (selectedIndex >= 0 && selectedIndex < options.Count)
                    {
                        valueSetter(options[selectedIndex]);
                    }
                }
                dropdown.RegisterValueChangedCallback(OnValueChanged);

                var eventBinding = new EventBinding(() => dropdown.UnregisterValueChangedCallback(OnValueChanged));
                AddBinding(element, eventBinding);
            }
        }
    }

    private static void AddBinding(VisualElement element, IBinding binding)
    {
        if (!bindings.ContainsKey(element))
        {
            bindings[element] = new List<IBinding>();
        }
        bindings[element].Add(binding);
    }

    public static void UpdateBindings(VisualElement element)
    {
        if (bindings.TryGetValue(element, out List<IBinding> elementBindings))
        {
            foreach (var binding in elementBindings)
            {
                binding.Update();
            }
        }
    }

    public static void UpdateAllBindings()
    {
        foreach (var elementBindings in bindings.Values)
        {
            foreach (var binding in elementBindings)
            {
                binding.Update();
            }
        }
    }

    public static void ClearBindings(VisualElement element)
    {
        if (bindings.TryGetValue(element, out List<IBinding> elementBindings))
        {
            foreach (var binding in elementBindings)
            {
                binding.Dispose();
            }
            bindings.Remove(element);
        }
    }

    public static void ClearAllBindings()
    {
        foreach (var elementBindings in bindings.Values)
        {
            foreach (var binding in elementBindings)
            {
                binding.Dispose();
            }
        }
        bindings.Clear();
    }
}