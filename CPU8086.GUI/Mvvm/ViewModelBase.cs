using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

// Kompatibilitaets-Shim fuer DevExpress.Mvvm.ViewModelBase.
// Bietet die genutzte API (GetValue/SetValue/Raise*/GetService) an,
// intern auf CommunityToolkit.Mvvm.ObservableObject aufgesetzt.
namespace DevExpress.Mvvm
{
    public abstract class ViewModelBase : ObservableObject, ISupportServices
    {
        private readonly Dictionary<string, object> _backingStore = new Dictionary<string, object>(StringComparer.Ordinal);
        private IServiceContainer _serviceContainer;

        public IServiceContainer ServiceContainer => _serviceContainer ??= new ServiceContainer();

        protected T GetService<T>() where T : class => ServiceContainer.GetService<T>();

        protected T GetService<T>(string key) where T : class => ServiceContainer.GetService<T>(key);

        // --- keyless POCO-Backing-Store (CallerMemberName) ---

        protected T GetValue<T>([CallerMemberName] string propertyName = null)
        {
            if (propertyName != null && _backingStore.TryGetValue(propertyName, out object value))
                return value is T typed ? typed : default;
            return default;
        }

        protected bool SetValue<T>(T value, [CallerMemberName] string propertyName = null)
            => SetValue(value, null, propertyName);

        protected bool SetValue<T>(T value, Action changedCallback, [CallerMemberName] string propertyName = null)
        {
            T current = GetValue<T>(propertyName);
            if (EqualityComparer<T>.Default.Equals(current, value))
                return false;
            _backingStore[propertyName] = value;
            OnPropertyChanged(propertyName);
            changedCallback?.Invoke();
            return true;
        }

        // --- feldbasiert (delegiert an ObservableObject.SetProperty) ---

        protected bool SetValue<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
            => SetValue(ref field, value, null, propertyName);

        protected bool SetValue<T>(ref T field, T value, Action changedCallback, [CallerMemberName] string propertyName = null)
        {
            bool changed = SetProperty(ref field, value, propertyName);
            if (changed)
                changedCallback?.Invoke();
            return changed;
        }

        // --- Notify-Helfer ---

        public void RaisePropertyChanged([CallerMemberName] string propertyName = null)
            => OnPropertyChanged(propertyName);

        public void RaisePropertiesChanged(params string[] propertyNames)
        {
            if (propertyNames == null)
                return;
            foreach (string propertyName in propertyNames)
                OnPropertyChanged(propertyName);
        }
    }
}
