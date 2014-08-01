using System;
using Newtonsoft.Json;

// ReSharper disable ClassNeverInstantiated.Global
// ReSharper disable UnusedAutoPropertyAccessor.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace SilogSolver
{
    #region ParametrosEntrada
    public class ParametrosDeEntrada
    {
        private double _nivelDeServicioMínimo;

        public ParametrosDeEntrada()
        {
            MateriasPrimas = new ParametrosMateriasPrimas();
            ProductoTerminado = new ParametrosProductoTerminado();
        }

        public int CantidadDeSemanas { get; set; }

        public PrimaPorCarga CalcularPrimaPorCargaIncompleta { get; set; }

        public double NivelDeServicioMínimo
        {
            get { return _nivelDeServicioMínimo; }
            set
            {
                if (value < 0 || value > 1)
                {
                    throw new ArgumentOutOfRangeException("NivelDeServicioMínimo debe estar entre 0 y 1");
                }
                _nivelDeServicioMínimo = value;
            }
        }

        public double UtilidadMínima { get; set; }

        public MaximizeParams Maximize { get; set; }

        public enum MaximizeParams
        {
            Utilidad,
            NivelDeServicio,
            Compuesto
        }

        public uint VentasPrevias { get; set; }
        public uint DemandasPrevias { get; set; }
        public uint InversionMantenimiento { get; set; }
        public double UtilidadAcumulada { get; set; }

        public enum PrimaPorCarga
        {
            MateriaPrima,
            ProductoTerminado,
            Both
        }

        public ParametrosMateriasPrimas MateriasPrimas { get; set; }

        public ParametrosProductoTerminado ProductoTerminado { get; set; }
        public ScoresEstimados ScoresEstimados { get; set; }
    }

    public class ScoresEstimados
    {
        public double UtilidadMinima { get; set; }
        public double UtilidadMaxima { get; set; }
        public double NivelDeServicioMinimo { get; set; }
        public double NivelDeServicioMaximo { get; set; }
    }

    public class ParametrosMateriasPrimas
    {
        public ParametrosMateriasPrimas()
        {
            Nikelen = new MateriaPrima();
            Alternic = new MateriaPrima();
            Progesic = new MateriaPrima();
        }

        public uint CapacidadActualAlmacen { get; set; }
        public uint CapacidadEnConstruccion { get; set; }
        public MateriaPrima Alternic { get; set; }
        public MateriaPrima Nikelen { get; set; }
        public MateriaPrima Progesic { get; set; }
    }
    public class MateriaPrima
    {
        public MateriaPrima()
        {
            EnTransito = new EnTransito();
        }
        [JsonIgnore]
        public uint Disponible { get { return AlmacenPropio + AlmacenAlquilado + Detencion; } }
        public uint AlmacenPropio { get; set; }
        public uint AlmacenAlquilado { get; set; }
        public uint Detencion { get; set; }
        public EnTransito EnTransito { get; set; }
    }
    public class EnTransito
    {
        public uint GetSemana(int index)
        {
            if (index == 1)
                return Semana1;
            if (index == 2)
                return Semana2;
            if (index == 3)
                return Semana3;
            if (index == 4)
                return Semana4;
            if (index == 5)
                return Semana5;
            if (index == 6)
                return Semana6;
            if (index == 7)
                return Semana7;
            if (index == 8)
                return Semana8;

            return 0;
        }

        public uint Semana1 { get; set; }
        public uint Semana2 { get; set; }
        public uint Semana3 { get; set; }
        public uint Semana4 { get; set; }
        public uint Semana5 { get; set; }
        public uint Semana6 { get; set; }
        public uint Semana7 { get; set; }
        public uint Semana8 { get; set; }
    }

    public class ParametrosProductoTerminado
    {
        public ParametrosProductoTerminado()
        {
            Monopolis = new ProductoTerminadoCiudad();
            Bipolis = new ProductoTerminadoCiudad();
            Tripolis = new ProductoTerminadoCiudad();
            Tetrapolis = new ProductoTerminadoCiudad();
            Metropolis = new ProductoTerminadoCiudad();
        }

        public ProductoTerminadoCiudad Monopolis { get; set; }
        public ProductoTerminadoCiudad Bipolis { get; set; }
        public ProductoTerminadoCiudad Tripolis { get; set; }
        public ProductoTerminadoCiudad Tetrapolis { get; set; }
        public ProductoTerminadoCiudad Metropolis { get; set; }
    }

    public class ProductoTerminadoCiudad
    {
        public ProductoTerminadoCiudad()
        {
            DemandaEstimada = new DemandaEstimada();
            EnTransito = new EnTransito();
        }

        public string CityName { get; set; }
        public DemandaEstimada DemandaEstimada { get; set; }
        public EnTransito EnTransito { get; set; }
        public int CapacidadActualAlmacen { get; set; }
        public int CapacidadEnConstruccion { get; set; }
        [JsonIgnore]
        public uint Disponible { get { return AlmacenPropio + Detencion; } }
        public uint AlmacenPropio { get; set; }
        public uint Detencion { get; set; }
    }

    public class DemandaEstimada
    {
        public uint GetSemana(int index)
        {
            if (index == 0)
                return Semana0;
            if (index == 1)
                return Semana1;
            if (index == 2)
                return Semana2;
            if (index == 3)
                return Semana3;
            if (index == 4)
                return Semana4;
            if (index == 5)
                return Semana5;
            if (index == 6)
                return Semana6;
            if (index == 7)
                return Semana7;
            if (index == 8)
                return Semana8;

            return 0;
        }

        public uint Semana0 { get; set; }
        public uint Semana1 { get; set; }
        public uint Semana2 { get; set; }
        public uint Semana3 { get; set; }
        public uint Semana4 { get; set; }
        public uint Semana5 { get; set; }
        public uint Semana6 { get; set; }
        public uint Semana7 { get; set; }
        public uint Semana8 { get; set; }
    }

    #endregion
}
