using System;
using System.Collections.Generic;
using NormalCAD.Core.DatabaseServices;
using ACadSharp;

namespace NormalCAD.Controller.Services.Converters
{
    public class ConverterService
    {
        private readonly Dictionary<Type, Func<ACadSharp.Entities.Entity, NormalCAD.Core.DatabaseServices.Entity?>> _acadToNormal = new();
        private readonly Dictionary<Type, Func<NormalCAD.Core.DatabaseServices.Entity, CadDocument, ACadSharp.Entities.Entity?>> _normalToAcad = new();
        private LayerConverter? _layerConverter;
        private VPortConverter? _vportConverter;

        public ConverterService()
        {
            Register(new LineConverter());
            Register(new CircleConverter());
            Register(new ArcConverter());
            Register(new PolylineConverter());
            Register(new BlockReferenceConverter());
            _layerConverter = new LayerConverter();
            _vportConverter = new VPortConverter();
        }

        public void RegisterLayerConverter(LayerConverter converter)
        {
            _layerConverter = converter;
        }

        public void Register<TNormal, TAcad>(EntityConverter<TNormal, TAcad> converter)
            where TNormal : NormalCAD.Core.DatabaseServices.Entity
            where TAcad : ACadSharp.Entities.Entity
        {
            _acadToNormal[typeof(TAcad)] = source => converter.ConvertToNormal((TAcad)source);
            _normalToAcad[typeof(TNormal)] = (source, cd) => converter.ConvertToAcad((TNormal)source, cd);
        }

        public NormalCAD.Core.DatabaseServices.Entity? ConvertToNormal(ACadSharp.Entities.Entity source)
        {
            var type = source.GetType();
            if (_acadToNormal.TryGetValue(type, out var converter))
                return converter(source);

            return null;
        }

        public ACadSharp.Entities.Entity? ConvertToAcad(NormalCAD.Core.DatabaseServices.Entity source, CadDocument cadDoc)
        {
            var type = source.GetType();
            if (_normalToAcad.TryGetValue(type, out var converter))
                return converter(source, cadDoc);

            return null;
        }

        public LayerTableRecord? ConvertLayerToNormal(ACadSharp.Tables.Layer source)
        {
            return _layerConverter?.ConvertToNormal(source);
        }

        public ACadSharp.Tables.Layer? ConvertLayerToAcad(LayerTableRecord source)
        {
            return _layerConverter?.ConvertToAcad(source);
        }

        public ViewportTableRecord? ConvertVPortToNormal(ACadSharp.Tables.VPort source)
        {
            return _vportConverter?.ConvertToNormal(source);
        }

        public ACadSharp.Tables.VPort? ConvertVPortToAcad(ViewportTableRecord source)
        {
            return _vportConverter?.ConvertToAcad(source);
        }

        public void ApplyVPortToAcad(ViewportTableRecord source, ACadSharp.Tables.VPort target)
        {
            _vportConverter?.ApplyToAcad(source, target);
        }
    }
}
