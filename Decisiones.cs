using System.Reflection;
using Microsoft.SolverFoundation.Services;

namespace SilogSolver
{
    #region Decisiones

    public class DecisionesSemanales : IDecisionesSemanales
    {
        public DecisionesSemanales(Model model, string prefix, string monopolis, string bipolis, string tripolis, string tetrapolis, string metropolis)
        {
            AgrandarAlmacen = new Decision(Domain.Boolean, prefix + "_agrandarAlmacen");
            AlquilarAlmacen = new Decision(Domain.Boolean, prefix + "_alquilarAlmacen");
            UnidadesAProducir = new Decision(Domain.IntegerNonnegative, prefix + "_unidadesAProducir");

            model.AddDecisions((Decision)AgrandarAlmacen, (Decision)AlquilarAlmacen, (Decision)UnidadesAProducir);

            Alternic = new DecisionesMateriaPrima(model, prefix + "_alternic");
            Nikelen = new DecisionesMateriaPrima(model, prefix + "_nikelen");
            Progesic = new DecisionesMateriaPrima(model, prefix + "_progesic");

            Monopolis = new DecisionesCiudad(model, prefix + "_" + monopolis);
            Bipolis = new DecisionesCiudad(model, prefix + "_" + bipolis);
            Tripolis = new DecisionesCiudad(model, prefix + "_" + tripolis);
            Tetrapolis = new DecisionesCiudad(model, prefix + "_" + tetrapolis);
            Metropolis = new DecisionesCiudad(model, prefix + "_" + metropolis);


            model.AddConstraint(prefix + "_constraint_unidadesAProducir", UnidadesAProducir <= 3370);
        }
        public Term AgrandarAlmacen { get; private set; }
        public Term AlquilarAlmacen { get; private set; }
        public Term UnidadesAProducir { get; set; }

        public IDecisionesMateriaPrima Nikelen { get; private set; }
        public IDecisionesMateriaPrima Progesic { get; private set; }
        public IDecisionesMateriaPrima Alternic { get; private set; }

        public IDecisionesCiudad Monopolis { get; private set; }
        public IDecisionesCiudad Bipolis { get; private set; }
        public IDecisionesCiudad Tripolis { get; private set; }
        public IDecisionesCiudad Tetrapolis { get; private set; }
        public IDecisionesCiudad Metropolis { get; private set; }
    }

    public class DecisionesMateriaPrima : IDecisionesMateriaPrima
    {
        public DecisionesMateriaPrima(Model model, string prefix)
        {
            Almacen = new DecisionesAlmacenMP(model, prefix);
            Transporte = new DecisionesMovimiento(model, prefix);
        }
        public IDecisionesAlmacenMP Almacen { get; private set; }
        public IDecisionesMovimiento Transporte { get; private set; }
    }

    public class DecisionesMovimiento : IDecisionesMovimiento
    {
        public DecisionesMovimiento(Model model, string prefix)
        {
            Aereo = new Decision(Domain.IntegerNonnegative, prefix + "_transporte_Aereo");
            Terrestre = new Decision(Domain.IntegerNonnegative, prefix + "_transporte_Terrestre");

            model.AddDecisions((Decision)Aereo, (Decision)Terrestre);
        }
        public Term Aereo { get; private set; }
        public Term Terrestre { get; private set; }


    }

    public class DecisionesAlmacenMP : IDecisionesAlmacenMP
    {
        public DecisionesAlmacenMP(Model model, string prefix)
        {
            AlmacenPropio = new Decision(Domain.IntegerNonnegative, prefix + "_AlmacenPropio");
            AlmacenAlquilado = new Decision(Domain.IntegerNonnegative, prefix + "_AlmacenAlquilado");
            Detencion = new Decision(Domain.IntegerNonnegative, prefix + "_Detencion");

            model.AddDecisions((Decision)AlmacenPropio, (Decision)AlmacenAlquilado, (Decision)Detencion);
        }

        public Term AlmacenPropio { get; private set; }
        public Term AlmacenAlquilado { get; private set; }
        public Term Detencion { get; private set; }
    }

    public class DecisionesCiudad : IDecisionesCiudad
    {
        public DecisionesCiudad(Model model, string prefix)
        {
            AgrandarAlmacen = new Decision(Domain.Boolean, prefix + "_agrandarAlmacen");

            AlmacenamientoPropio = new Decision(Domain.IntegerNonnegative, prefix + "_almacenPropio");
            Detencion = new Decision(Domain.IntegerNonnegative, prefix + "_Detencion");


            model.AddDecisions((Decision)AgrandarAlmacen, (Decision)AlmacenamientoPropio, (Decision)Detencion);

            Transporte = new DecisionesMovimiento(model, prefix);
        }

        public IDecisionesMovimiento Transporte { get; private set; }

        public Term AgrandarAlmacen { get; private set; }
        public Term AlmacenamientoPropio { get; private set; }
        public Term Detencion { get; private set; }
        public Term Ventas { get; set; }
    }

    #endregion

    #region Nulls
    public class NullDecisionesSemanales : IDecisionesSemanales
    {
        public NullDecisionesSemanales()
        {
            AgrandarAlmacen = 0;
            AlquilarAlmacen = 0;
            UnidadesAProducir = 0;


            Nikelen = new NullDecisionesMateriaPrima();
            Alternic = new NullDecisionesMateriaPrima();
            Progesic = new NullDecisionesMateriaPrima();

            Monopolis = new NullDecisionesCiudad();
            Bipolis = new NullDecisionesCiudad();
            Tripolis = new NullDecisionesCiudad();
            Tetrapolis = new NullDecisionesCiudad();
            Metropolis = new NullDecisionesCiudad();
        }
        public Term AgrandarAlmacen { get; private set; }
        public Term AlquilarAlmacen { get; private set; }
        public Term UnidadesAProducir { get; set; }

        public IDecisionesMateriaPrima Nikelen { get; private set; }
        public IDecisionesMateriaPrima Progesic { get; private set; }
        public IDecisionesMateriaPrima Alternic { get; private set; }

        public IDecisionesCiudad Monopolis { get; private set; }
        public IDecisionesCiudad Bipolis { get; private set; }
        public IDecisionesCiudad Tripolis { get; private set; }
        public IDecisionesCiudad Tetrapolis { get; private set; }
        public IDecisionesCiudad Metropolis { get; private set; }
    }

    public class NullDecisionesMateriaPrima : IDecisionesMateriaPrima
    {
        public NullDecisionesMateriaPrima()
        {
            Almacen = new NullDecisionesAlmacenMP();
            Transporte = new NullDecisionesMovimiento();
        }

        public IDecisionesAlmacenMP Almacen { get; private set; }
        public IDecisionesMovimiento Transporte { get; private set; }
    }

    public class NullDecisionesMovimiento : IDecisionesMovimiento
    {
        public NullDecisionesMovimiento()
        {
            Aereo = 0;
            Terrestre = 0;
        }
        public Term Aereo { get; private set; }
        public Term Terrestre { get; private set; }
    }

    public class NullDecisionesAlmacenMP : IDecisionesAlmacenMP
    {
        public NullDecisionesAlmacenMP()
        {
            AlmacenPropio = 0;
            AlmacenAlquilado = 0;
            Detencion = 0;
        }
        public Term AlmacenPropio { get; private set; }
        public Term AlmacenAlquilado { get; private set; }
        public Term Detencion { get; private set; }

    }

    public class NullDecisionesCiudad : IDecisionesCiudad
    {
        public NullDecisionesCiudad()
        {
            AgrandarAlmacen = 0;
            AlmacenamientoPropio = 0;
            Detencion = 0;
            Transporte = new NullDecisionesMovimiento();
        }

        public IDecisionesMovimiento Transporte { get; private set; }

        public Term AgrandarAlmacen { get; private set; }
        public Term AlmacenamientoPropio { get; private set; }
        public Term Detencion { get; private set; }
        public Term Ventas { get; set; }
    }
    #endregion

    #region Interfaces
    public interface IDecisionesSemanales
    {
        Term AgrandarAlmacen { get; }
        Term AlquilarAlmacen { get; }
        Term UnidadesAProducir { get; }

        IDecisionesMateriaPrima Nikelen { get; }
        IDecisionesMateriaPrima Progesic { get; }
        IDecisionesMateriaPrima Alternic { get; }

        IDecisionesCiudad Monopolis { get; }
        IDecisionesCiudad Bipolis { get; }
        IDecisionesCiudad Tripolis { get; }
        IDecisionesCiudad Tetrapolis { get; }
        IDecisionesCiudad Metropolis { get; }
    }

    public interface IDecisionesMovimiento
    {
        Term Aereo { get; }
        Term Terrestre { get; }
    }

    public interface IDecisionesAlmacenMP
    {
        Term AlmacenPropio { get; }
        Term AlmacenAlquilado { get; }
        Term Detencion { get; }
    }

    public interface IDecisionesMateriaPrima
    {
        IDecisionesAlmacenMP Almacen { get; }

        IDecisionesMovimiento Transporte { get; }
    }

    public interface IDecisionesCiudad
    {
        IDecisionesMovimiento Transporte { get; }
        Term AgrandarAlmacen { get; }
        Term AlmacenamientoPropio { get; }
        Term Detencion { get; }
        Term Ventas { get; set; }
    }
    #endregion
}
