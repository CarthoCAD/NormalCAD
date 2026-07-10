using System;
using System.Collections.Generic;
using System.Linq;
using NormalCAD.Core.DatabaseServices;
using NormalCAD.Resources;

namespace NormalCAD.Controller.Providers
{
    public class EntityPropertyManager
    {
        private readonly Dictionary<Type, IEntityPropertyProvider> _providers = new();
        private readonly EntityPropertyProvider _entity = new();

        private static string VariesValue => PanelResources.Get("PROPERTYPALETTE.VALUE.VARIES");

        public EntityPropertyManager()
        {
            Register<Line>(new LinePropertyProvider());
            Register<Circle>(new CirclePropertyProvider());
            Register<Arc>(new ArcPropertyProvider());
            Register<Polyline>(new PolylinePropertyProvider());
        }

        private void Register<T>(IEntityPropertyProvider provider) where T : Entity
        {
            _providers[typeof(T)] = provider;
        }

        public string GetDisplayName(Type entityType) =>
            _providers.TryGetValue(entityType, out var provider) ? provider.DisplayName : entityType.Name;

        public IReadOnlyList<PropertyDescriptor> GetProperties(Entity entity)
        {
            var list = new List<PropertyDescriptor>();
            list.AddRange(_entity.GetProperties(entity));
            if (_providers.TryGetValue(entity.GetType(), out var provider))
                list.AddRange(provider.GetProperties(entity));
            return list;
        }

        public IReadOnlyList<PropertyDescriptor> GetMergedProperties(IReadOnlyList<Entity> entities)
        {
            if (entities.Count == 0)
                return Array.Empty<PropertyDescriptor>();
            if (entities.Count == 1)
                return GetProperties(entities[0]);

            var lists = entities.Select(GetProperties).ToList();
            var firstList = lists[0];
            var merged = new List<PropertyDescriptor>();

            foreach (var descriptor in firstList)
            {
                if (descriptor.SingleSelectionOnly)
                    continue;

                var matches = new List<PropertyDescriptor> { descriptor };

                for (int i = 1; i < lists.Count; i++)
                {
                    var match = lists[i].FirstOrDefault(d =>
                        d.DisplayName == descriptor.DisplayName &&
                        d.Category == descriptor.Category &&
                        d.PropertyType == descriptor.PropertyType);
                    if (match == null)
                        break;
                    matches.Add(match);
                }

                if (matches.Count != entities.Count)
                    continue;

                bool isReadOnly = matches.Any(d => d.TrySetValue == null);

                merged.Add(new PropertyDescriptor
                {
                    Category = descriptor.Category,
                    DisplayName = descriptor.DisplayName,
                    PropertyType = descriptor.PropertyType,
                    Order = descriptor.Order,
                    ComboOptions = descriptor.ComboOptions,
                    GetValue = () =>
                    {
                        var values = matches.Select(d => d.GetValue()).Distinct().ToList();
                        return values.Count == 1 ? values[0] : VariesValue;
                    },
                    TrySetValue = isReadOnly ? null : v =>
                    {
                        bool allOk = true;
                        foreach (var d in matches)
                        {
                            if (d.TrySetValue != null && !d.TrySetValue(v))
                                allOk = false;
                        }
                        return allOk;
                    }
                });
            }

            return merged;
        }
    }
}
