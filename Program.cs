using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.SolverFoundation.Services;
using Newtonsoft.Json;

// ReSharper disable InconsistentNaming


namespace SilogSolver
{
    public static class Helepr
    {
        public static Decision AddVentasSemana(this Model model, int semana, string ciudad, Term demanda, Term disponible)
        {
            var ventas = new Decision(Domain.IntegerNonnegative, "s" + semana + "_ventas_" + ciudad);
            model.AddDecision(ventas);
            model.AddConstraint("s" + semana + "_ventas_" + ciudad + "_demanda", ventas <= demanda);
            model.AddConstraint("s" + semana + "_ventas_" + ciudad + "_disponibilidad", ventas <= disponible);
            model.AddConstraint("s" + semana + "_ventas_" + ciudad + "_min", ventas >= demanda * 0.81);

            return ventas;
        }


        public static Term AddRestriccionAlmacenProductoTerminado(this Model model, int semana, Term disponibilidad, Term capacidadAlmacen, IDecisionesPTCiudad ciudad, string city)
        {

            model.AddConstraint("s" + semana + "_cantidadTotal_PT_" + city, disponibilidad == ciudad.AlmacenamientoPropio + ciudad.Detencion);
            model.AddConstraint("s" + semana + "_capacidadAlmacen_PT_" + city, capacidadAlmacen >= ciudad.AlmacenamientoPropio);
            //Falta el costo de acarreo
            return SilogParams.CostoFijoAlmacenPropioPT * capacidadAlmacen + SilogParams.CostoVariableAlmacenPropioPT * ciudad.AlmacenamientoPropio + SilogParams.CostoDetencionPT * ciudad.Detencion;
        }

        public static Term AddContainerIncompletoTerrestreMP(this Model model, ParametrosDeEntrada.PrimaPorCarga prima, Term total, int semana, string transp)
        {
            if (prima == ParametrosDeEntrada.PrimaPorCarga.Both ||
                prima == ParametrosDeEntrada.PrimaPorCarga.MateriaPrima)
            {
                var container = new Decision(Domain.IntegerNonnegative, "s" + semana + "_container_terrestre_" + transp);
                var parcial = new Decision(Domain.IntegerNonnegative, "s" + semana + "_parcial_terrestre_" + transp);

                model.AddDecisions(container, parcial);
                model.AddConstraint("s" + semana + "_containerTotal_terrestre_" + transp,
                    container * SilogParams.CapContenedorTerrestreMP + parcial == total);

                return parcial;
            }
            return 0;
        }

        public static Term AddContainerIncompletoAereoMP(this Model model, ParametrosDeEntrada.PrimaPorCarga prima, Term total, int semana, string transp)
        {
            if (prima == ParametrosDeEntrada.PrimaPorCarga.Both ||
                prima == ParametrosDeEntrada.PrimaPorCarga.MateriaPrima)
            {
                var container = new Decision(Domain.IntegerNonnegative, "s" + semana + "_container_aereo_" + transp);
                var parcial = new Decision(Domain.IntegerNonnegative, "s" + semana + "_parcial_aereo_" + transp);

                model.AddDecisions(container, parcial);
                model.AddConstraint("s" + semana + "_containerTotal_aereo_" + transp,
                    container * SilogParams.CapContenedorAereoMP + parcial == total);

                return parcial;
            }
            return 0;
        }

        public static Term AddContainerIncompletoTerrestrePT(this Model model, ParametrosDeEntrada.PrimaPorCarga prima, Term total, int semana, string transp)
        {
            if (prima == ParametrosDeEntrada.PrimaPorCarga.Both ||
                prima == ParametrosDeEntrada.PrimaPorCarga.ProductoTerminado)
            {
                var container = new Decision(Domain.IntegerNonnegative, "s" + semana + "_container_terrestre_" + transp);
                var parcial = new Decision(Domain.IntegerNonnegative, "s" + semana + "_parcial_terrestre_" + transp);

                model.AddDecisions(container, parcial);
                model.AddConstraint("s" + semana + "_containerTotal_terrestre_" + transp,
                    container * SilogParams.CapContenedorTerrestrePT + parcial == total);

                return parcial;
            }
            return 0;
        }

        public static Term AddContainerIncompletoAereoPT(this Model model, ParametrosDeEntrada.PrimaPorCarga prima, Term total, int semana, string transp)
        {
            if (prima == ParametrosDeEntrada.PrimaPorCarga.Both ||
                prima == ParametrosDeEntrada.PrimaPorCarga.ProductoTerminado)
            {
                var container = new Decision(Domain.IntegerNonnegative, "s" + semana + "_container_aereo_" + transp);
                var parcial = new Decision(Domain.IntegerNonnegative, "s" + semana + "_parcial_aereo_" + transp);

                model.AddDecisions(container, parcial);
                model.AddConstraint("s" + semana + "_containerTotal_aereo_" + transp,
                    container * SilogParams.CapContenedorAereoPT + parcial == total);
                return parcial;
            }
            return 0;
        }
    }

    public static class SilogParams
    {
        public const uint CapContenedorAereoMP = 6500;
        public const uint CapContenedorTerrestreMP = 14000;

        public const uint CapContenedorAereoPT = 130;
        public const uint CapContenedorTerrestrePT = 280;

        public const double CostoExpansionAlmacenMP = 700;
        public const double CapacidadAdicionalExpansionMP = 4000;

        public const double CapacidadAdicionalExpansionPT = 50;
        public const double CostoExpansionAlmacenPT = 150;

        public const double CostoTransporteTierraAlternic = 0.2;
        public const double CostoTransporteAereoAlternic = 0.3;

        public const double CostoTransporteTierraNikelen = 0.15;
        public const double CostoTransporteAereoNikelen = 0.25;

        public const double CostoTransporteTierraProgesic = 0.30;
        public const double CostoTransporteAereoProgesic = 1;

        public const double CostoOrdenamientoMP = 50;

        public const double CostoFijoProduccion = 100;
        public const double CostoVariableProduccion = 0.225;

        public const double CostoFijoAlmacenPropioMP = 0.02;
        public const double CostoVariableAlmacenPropioMP = 0.02;
        public const double CostoAcarreoAlmacenPropioMP = 0.06;

        public const double CostoFijoAlmacenPropioPT = 0.4;
        public const double CostoVariableAlmacenPropioPT = 0.5;
        public const double CostoAcarreoAlmacenPropioPT = 2;

        public const double CostoFijoAlmacenAlquilado = 50;
        public const double CostoVariableAlmacenAlquilado = 0.40;

        public const double CostoDetencionMP = 0.8;
        public const double CostoDetencionPT = 5.0;


        public const uint AlternicPorProductoTerminado = 12;
        public const uint NikelenPorProductoTerminado = 8;
        public const uint ProgesicPorProductoTerminado = 4;
        public const uint PrecioDeVenta = 30;

        public const double CostoTransporteTierra1000 = 7;
        public const double CostoTransporteAereo1000 = 12;

        public const double CostoTransporteTierra1400 = 9;
        public const double CostoTransporteAereo1400 = 15;

        public const double CostoTransporteTierra700 = 5;
        public const double CostoTransporteAereo700 = 8;


        public const uint CapacidadAlmacenAlquilado = 4000;
        public const double PrimaCargaIncompleta = 0.25;


        public const double ConsumoMaximoDeDisponible = 1;

    }

    public class ListDecisionesSemanales : List<IDecisionesSemanales>
    {
        public ListDecisionesSemanales(Model model, int total)
        {
            for (int i = 0; i < total; i++)
            {
                Add(new DecisionesSemanales(model, "s" + i));
            }

        }

        public new IDecisionesSemanales this[int index]
        {
            get
            {
                if (index < 0)
                {
                    return new NullDecisionesSemanales();
                }
                return base[index];
            }
            set { base[index] = value; }
        }
    }

    class Program
    {
        static void Main(string[] args)
        {
            SolverContext context = SolverContext.GetContext();
            Model model = context.CreateModel();

            string path;

            if (args.Length < 1)
            {
                path = "params.json";
            }
            else
            {
                path = args[0];
            }


            var data = System.IO.File.ReadAllText(path);
            var parametrosEntrada = JsonConvert.DeserializeObject<ParametrosDeEntrada>(data);
            var numSemanas = parametrosEntrada.CantidadDeSemanas;

            if (numSemanas < 1)
            {
                Console.WriteLine("No. de Semanas Inválidos");
                Console.ReadLine();
                return;
            }

            #region Variables

            var d_semana = new ListDecisionesSemanales(model, numSemanas + 1);

            Term ventasTotal;
            Term demandaTotal;

            #region Produccion
            var consumo_alternic = new Term[numSemanas + 1];
            var disponible_alternic = new Term[numSemanas + 1];

            var consumo_nikelen = new Term[numSemanas + 1];
            var disponible_nikelen = new Term[numSemanas + 1];

            var consumo_progesic = new Term[numSemanas + 1];
            var disponible_progesic = new Term[numSemanas + 1];

            #endregion

            #region Ventas

            var salida_Monopolis = new Term[numSemanas + 1];
            var disponible_Monopolis = new Term[numSemanas + 1];
            var disponible_Bipolis = new Term[numSemanas + 1];
            var disponible_Tripolis = new Term[numSemanas + 1];
            var disponible_Tetrapolis = new Term[numSemanas + 1];
            var disponible_Metropolis = new Term[numSemanas + 1];

            var Ventas = new Term[numSemanas + 1];
            var UnidadesVendidas = new Term[numSemanas + 1];
            var DemandaTotalEstimada = new Term[numSemanas + 1];
            var NivelDeServicio = new Decision[numSemanas + 1];

            #endregion

            #region Costos

            var costos_Produccion = new Term[numSemanas + 1];

            var capacidad_almacenPropio_MP = new Term[numSemanas + 1];
            var capacidad_almacenAlquilado_MP = new Term[numSemanas + 1];


            var costo_almacenamiento_mp = new Term[numSemanas + 1];

            var capacidad_almacen_PT_Monopolis = new Term[numSemanas + 1];
            var costo_almacenamiento_Monopolis = new Term[numSemanas + 1];
            var capacidad_almacen_PT_Bipolis = new Term[numSemanas + 1];
            var costo_almacenamiento_Bipolis = new Term[numSemanas + 1];
            var capacidad_almacen_PT_Tripolis = new Term[numSemanas + 1];
            var costo_almacenamiento_Tripolis = new Term[numSemanas + 1];
            var capacidad_almacen_PT_Tetrapolis = new Term[numSemanas + 1];
            var costo_almacenamiento_Tetrapolis = new Term[numSemanas + 1];
            var capacidad_almacen_PT_Metropolis = new Term[numSemanas + 1];
            var costo_almacenamiento_Metropolis = new Term[numSemanas + 1];
            var costos_Almacenamiento = new Term[numSemanas + 1];



            var costo_parcial_terrestre_alternic = new Term[numSemanas + 1];
            var costo_parcial_aereo_alternic = new Term[numSemanas + 1];
            var costo_transporte_alternic = new Term[numSemanas + 1];
            var costo_parcial_terrestre_nikelen = new Term[numSemanas + 1];
            var costo_parcial_aereo_nikelen = new Term[numSemanas + 1];
            var costo_transporte_nikelen = new Term[numSemanas + 1];
            var costo_parcial_terrestre_progesic = new Term[numSemanas + 1];
            var costo_parcial_aereo_progesic = new Term[numSemanas + 1];
            var costo_transporte_progesic = new Term[numSemanas + 1];

            var costo_transporte_MP = new Term[numSemanas + 1];


            var costo_parcial_terrestre_bipolis = new Term[numSemanas + 1];
            var costo_parcial_aereo_bipolis = new Term[numSemanas + 1];
            var costo_transporte_bipolis = new Term[numSemanas + 1];
            var costo_parcial_terrestre_tripolis = new Term[numSemanas + 1];
            var costo_parcial_aereo_tripolis = new Term[numSemanas + 1];
            var costo_transporte_tripolis = new Term[numSemanas + 1];
            var costo_parcial_terrestre_tetrapolis = new Term[numSemanas + 1];
            var costo_parcial_aereo_tetrapolis = new Term[numSemanas + 1];
            var costo_transporte_tetrapolis = new Term[numSemanas + 1];
            var costo_parcial_terrestre_metropolis = new Term[numSemanas + 1];
            var costo_parcial_aereo_metropolis = new Term[numSemanas + 1];
            var costo_transporte_metropolis = new Term[numSemanas + 1];
            var costo_transporte_PT = new Term[numSemanas + 1];
            var costos_Transporte = new Term[numSemanas + 1];

            var Gastos = new Term[numSemanas + 1];


            var Utilidad = new Term[numSemanas];

            #endregion

            #endregion

            #region Caso Especial Semana 0

            #region Produccion

            consumo_alternic[0] = SilogParams.AlternicPorProductoTerminado * d_semana[0].UnidadesAProducir;
            disponible_alternic[0] = parametrosEntrada.MateriasPrimas.Alternic.Disponible;
            model.AddConstraint("s" + 0 + "_consumoMax_alternic", consumo_alternic[0] <= disponible_alternic[0]);

            consumo_nikelen[0] = SilogParams.NikelenPorProductoTerminado * d_semana[0].UnidadesAProducir;
            disponible_nikelen[0] = parametrosEntrada.MateriasPrimas.Nikelen.Disponible;
            model.AddConstraint("s" + 0 + "_consumoMax_nikelen", consumo_nikelen[0] <= disponible_nikelen[0]);

            consumo_progesic[0] = SilogParams.ProgesicPorProductoTerminado * d_semana[0].UnidadesAProducir;
            disponible_progesic[0] = parametrosEntrada.MateriasPrimas.Progesic.Disponible;
            model.AddConstraint("s" + 0 + "_consumoMax_progesic", consumo_progesic[0] <= disponible_progesic[0]);

            #endregion

            #region Ventas

            salida_Monopolis[0] = (d_semana[0].Bipolis.Transporte.Aereo +
                                             d_semana[0].Bipolis.Transporte.Terrestre +
                                             d_semana[0].Tripolis.Transporte.Aereo +
                                             d_semana[0].Tripolis.Transporte.Terrestre +
                                             d_semana[0].Tetrapolis.Transporte.Aereo +
                                             d_semana[0].Tetrapolis.Transporte.Terrestre +
                                             d_semana[0].Metropolis.Transporte.Aereo +
                                             d_semana[0].Metropolis.Transporte.Terrestre);

            disponible_Monopolis[0] = parametrosEntrada.ProductoTerminado.Monopolis.Disponible - salida_Monopolis[0];
            d_semana[0].Monopolis.Ventas = model.AddVentasSemana(0, "monopolis", parametrosEntrada.ProductoTerminado.Monopolis.DemandaEstimada.GetSemana(0), disponible_Monopolis[0]);

            disponible_Bipolis[0] = parametrosEntrada.ProductoTerminado.Bipolis.Disponible;
            d_semana[0].Bipolis.Ventas = model.AddVentasSemana(0, "bipolis", parametrosEntrada.ProductoTerminado.Bipolis.DemandaEstimada.GetSemana(0), disponible_Bipolis[0]);

            disponible_Tripolis[0] = parametrosEntrada.ProductoTerminado.Tripolis.Disponible;
            d_semana[0].Tripolis.Ventas = model.AddVentasSemana(0, "tripolis", parametrosEntrada.ProductoTerminado.Tripolis.DemandaEstimada.GetSemana(0), disponible_Tripolis[0]);

            disponible_Tetrapolis[0] = parametrosEntrada.ProductoTerminado.Tetrapolis.Disponible;
            d_semana[0].Tetrapolis.Ventas = model.AddVentasSemana(0, "tetrapolis", parametrosEntrada.ProductoTerminado.Tetrapolis.DemandaEstimada.GetSemana(0), disponible_Tetrapolis[0]);

            disponible_Metropolis[0] = parametrosEntrada.ProductoTerminado.Metropolis.Disponible;
            d_semana[0].Metropolis.Ventas = model.AddVentasSemana(0, "metropolis", parametrosEntrada.ProductoTerminado.Metropolis.DemandaEstimada.GetSemana(0), disponible_Metropolis[0]);

            UnidadesVendidas[0] = d_semana[0].Monopolis.Ventas +
                d_semana[0].Bipolis.Ventas +
                d_semana[0].Tripolis.Ventas +
                d_semana[0].Tetrapolis.Ventas +
                d_semana[0].Metropolis.Ventas;



            DemandaTotalEstimada[0] =
                 parametrosEntrada.ProductoTerminado.Monopolis.DemandaEstimada.GetSemana(0) +
                 parametrosEntrada.ProductoTerminado.Bipolis.DemandaEstimada.GetSemana(0) +
                 parametrosEntrada.ProductoTerminado.Tripolis.DemandaEstimada.GetSemana(0) +
                 parametrosEntrada.ProductoTerminado.Tetrapolis.DemandaEstimada.GetSemana(0) +
                 parametrosEntrada.ProductoTerminado.Metropolis.DemandaEstimada.GetSemana(0);

            NivelDeServicio[0] = new Decision(Domain.Real, "s0_nivelDeServicio");
            model.AddDecision(NivelDeServicio[0]);


            model.AddConstraint("s0_constraint_nivelDeServicio", NivelDeServicio[0] == UnidadesVendidas[0] / DemandaTotalEstimada[0]);

            if (parametrosEntrada.NivelDeServicioMínimo > 0)
            {
                model.AddConstraint("s0_min_nivelDeServicio", UnidadesVendidas[0] >= DemandaTotalEstimada[0] * parametrosEntrada.NivelDeServicioMínimo);
            }

            ventasTotal = UnidadesVendidas[0];
            demandaTotal = DemandaTotalEstimada[0];

            Ventas[0] = UnidadesVendidas[0] * SilogParams.PrecioDeVenta;


            #endregion

            #region Gastos

            costos_Produccion[0] = SilogParams.CostoFijoProduccion + SilogParams.CostoVariableProduccion * d_semana[0].UnidadesAProducir +
                                             consumo_alternic[0] * SilogParams.CostoAcarreoAlmacenPropioMP +
                                             consumo_nikelen[0] * SilogParams.CostoAcarreoAlmacenPropioMP +
                                             consumo_progesic[0] * SilogParams.CostoAcarreoAlmacenPropioMP;

            #region Costos Almacen

            capacidad_almacenPropio_MP[0] = parametrosEntrada.MateriasPrimas.CapacidadActualAlmacen;
            capacidad_almacenAlquilado_MP[0] = d_semana[0].AlquilarAlmacen * SilogParams.CapacidadAlmacenAlquilado;

            model.AddConstraint("s" + 0 + "_cantidadAlmancen_alternic", disponible_alternic[0] == d_semana[0].Alternic.AlmacenPropio + d_semana[0].Alternic.AlmacenAlquilado + d_semana[0].Alternic.Detencion);
            model.AddConstraint("s" + 0 + "_cantidadAlmancen_nikelen", disponible_nikelen[0] == d_semana[0].Nikelen.AlmacenPropio + d_semana[0].Nikelen.AlmacenAlquilado + d_semana[0].Nikelen.Detencion);
            model.AddConstraint("s" + 0 + "_cantidadAlmancen_progesic", disponible_progesic[0] == d_semana[0].Progesic.AlmacenPropio + d_semana[0].Progesic.AlmacenAlquilado + d_semana[0].Progesic.Detencion);

            model.AddConstraint("s" + 0 + "_capacidadAlmancenPropio_MP", capacidad_almacenPropio_MP[0] >= d_semana[0].Alternic.AlmacenPropio + d_semana[0].Nikelen.AlmacenPropio + d_semana[0].Progesic.AlmacenPropio);
            model.AddConstraint("s" + 0 + "_capacidadAlmancenAlquilado_MP", capacidad_almacenAlquilado_MP[0] >= d_semana[0].Alternic.AlmacenAlquilado + d_semana[0].Nikelen.AlmacenAlquilado + d_semana[0].Progesic.AlmacenAlquilado);

            costo_almacenamiento_mp[0] =
                d_semana[0].AgrandarAlmacen * SilogParams.CostoExpansionAlmacenMP +
                SilogParams.CostoFijoAlmacenPropioMP * capacidad_almacenPropio_MP[0] +
                //Costo almacenamiento propio
                (d_semana[0].Alternic.AlmacenPropio * SilogParams.CostoVariableAlmacenPropioMP) +
                (d_semana[0].Nikelen.AlmacenPropio * SilogParams.CostoVariableAlmacenPropioMP) +
                (d_semana[0].Progesic.AlmacenPropio * SilogParams.CostoVariableAlmacenPropioMP) +
                //Costo almacenamiento alquilado
                d_semana[0].AlquilarAlmacen * SilogParams.CostoFijoAlmacenAlquilado +
                (d_semana[0].Alternic.AlmacenAlquilado * SilogParams.CostoVariableAlmacenAlquilado) +
                (d_semana[0].Nikelen.AlmacenAlquilado * SilogParams.CostoVariableAlmacenAlquilado) +
                (d_semana[0].Progesic.AlmacenAlquilado * SilogParams.CostoVariableAlmacenAlquilado) +
                //Costo en Detencion
                (d_semana[0].Alternic.Detencion * SilogParams.CostoDetencionMP) +
                (d_semana[0].Nikelen.Detencion * SilogParams.CostoDetencionMP) +
                (d_semana[0].Progesic.Detencion * SilogParams.CostoDetencionMP);


            capacidad_almacen_PT_Monopolis[0] = parametrosEntrada.ProductoTerminado.Monopolis.CapacidadActualAlmacen;
            costo_almacenamiento_Monopolis[0] =
                d_semana[0].Monopolis.AgrandarAlmacen * SilogParams.CostoExpansionAlmacenPT +
                d_semana[0].Monopolis.Ventas * SilogParams.CostoAcarreoAlmacenPropioPT +
                model.AddRestriccionAlmacenProductoTerminado(0, disponible_Monopolis[0], capacidad_almacen_PT_Monopolis[0], d_semana[0].Monopolis, "monopolis");

            capacidad_almacen_PT_Bipolis[0] = parametrosEntrada.ProductoTerminado.Bipolis.CapacidadActualAlmacen;
            costo_almacenamiento_Bipolis[0] =
                d_semana[0].Bipolis.AgrandarAlmacen * SilogParams.CostoExpansionAlmacenPT +
                d_semana[0].Bipolis.Ventas * SilogParams.CostoAcarreoAlmacenPropioPT +
                model.AddRestriccionAlmacenProductoTerminado(0, disponible_Bipolis[0], capacidad_almacen_PT_Bipolis[0], d_semana[0].Bipolis, "bipolis");

            capacidad_almacen_PT_Tripolis[0] = parametrosEntrada.ProductoTerminado.Tripolis.CapacidadActualAlmacen;
            costo_almacenamiento_Tripolis[0] =
                d_semana[0].Tripolis.AgrandarAlmacen * SilogParams.CostoExpansionAlmacenPT +
                d_semana[0].Tripolis.Ventas * SilogParams.CostoAcarreoAlmacenPropioPT +
                model.AddRestriccionAlmacenProductoTerminado(0, disponible_Tripolis[0], capacidad_almacen_PT_Tripolis[0], d_semana[0].Tripolis, "tripolis");

            capacidad_almacen_PT_Tetrapolis[0] = parametrosEntrada.ProductoTerminado.Tetrapolis.CapacidadActualAlmacen;
            costo_almacenamiento_Tetrapolis[0] =
                d_semana[0].Tetrapolis.AgrandarAlmacen * SilogParams.CostoExpansionAlmacenPT +
                d_semana[0].Tetrapolis.Ventas * SilogParams.CostoAcarreoAlmacenPropioPT +
                model.AddRestriccionAlmacenProductoTerminado(0, disponible_Tetrapolis[0], capacidad_almacen_PT_Tetrapolis[0], d_semana[0].Tetrapolis, "tetrapolis");

            capacidad_almacen_PT_Metropolis[0] = parametrosEntrada.ProductoTerminado.Metropolis.CapacidadActualAlmacen;
            costo_almacenamiento_Metropolis[0] =
                d_semana[0].Metropolis.AgrandarAlmacen * SilogParams.CostoExpansionAlmacenPT +
                d_semana[0].Metropolis.Ventas * SilogParams.CostoAcarreoAlmacenPropioPT +
                model.AddRestriccionAlmacenProductoTerminado(0, disponible_Metropolis[0], capacidad_almacen_PT_Metropolis[0], d_semana[0].Metropolis, "metropolis");

            costos_Almacenamiento[0] =
                costo_almacenamiento_mp[0] +
                costo_almacenamiento_Monopolis[0] +
                costo_almacenamiento_Bipolis[0] +
                costo_almacenamiento_Tripolis[0] +
                costo_almacenamiento_Tetrapolis[0] +
                costo_almacenamiento_Metropolis[0];

            #endregion

            #region Costos Transporte

            #region Materia Prima

            costo_parcial_terrestre_alternic[0] = model.AddContainerIncompletoTerrestreMP(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[0].Alternic.Terrestre, 0, "alternic");
            costo_parcial_aereo_alternic[0] = model.AddContainerIncompletoAereoMP(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[0].Alternic.Aereo, 0, "alternic");
            costo_transporte_alternic[0] = (SilogParams.CostoTransporteTierraAlternic * d_semana[0].Alternic.Terrestre + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_terrestre_alternic[0] * SilogParams.CostoTransporteTierraAlternic) * SilogParams.PrimaCargaIncompleta) +
                                                     (SilogParams.CostoTransporteAereoAlternic * d_semana[0].Alternic.Aereo + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_aereo_alternic[0] * SilogParams.CostoTransporteAereoAlternic) * SilogParams.PrimaCargaIncompleta);

            costo_parcial_terrestre_nikelen[0] = model.AddContainerIncompletoTerrestreMP(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[0].Nikelen.Terrestre, 0, "nikelen");
            costo_parcial_aereo_nikelen[0] = model.AddContainerIncompletoAereoMP(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[0].Nikelen.Aereo, 0, "nikelen");
            costo_transporte_nikelen[0] = (SilogParams.CostoTransporteTierraNikelen * d_semana[0].Nikelen.Terrestre + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_terrestre_nikelen[0] * SilogParams.CostoTransporteTierraNikelen) * SilogParams.PrimaCargaIncompleta) +
                                                    (SilogParams.CostoTransporteAereoNikelen * d_semana[0].Nikelen.Aereo + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_aereo_nikelen[0] * SilogParams.CostoTransporteAereoNikelen) * SilogParams.PrimaCargaIncompleta);

            costo_parcial_terrestre_progesic[0] = model.AddContainerIncompletoTerrestreMP(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[0].Progesic.Terrestre, 0, "progesic");
            costo_parcial_aereo_progesic[0] = model.AddContainerIncompletoAereoMP(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[0].Progesic.Aereo, 0, "progesic");
            costo_transporte_progesic[0] = (SilogParams.CostoTransporteTierraProgesic * d_semana[0].Progesic.Terrestre + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_terrestre_progesic[0] * SilogParams.CostoTransporteTierraProgesic) * SilogParams.PrimaCargaIncompleta) +
                                                     (SilogParams.CostoTransporteAereoProgesic * d_semana[0].Progesic.Aereo + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_aereo_progesic[0] * SilogParams.CostoTransporteAereoProgesic) * SilogParams.PrimaCargaIncompleta);

            costo_transporte_MP[0] = costo_transporte_alternic[0] +
                                               costo_transporte_nikelen[0] +
                                               costo_transporte_progesic[0];

            #endregion


            #region Producto Terminado

            costo_parcial_terrestre_bipolis[0] = model.AddContainerIncompletoTerrestrePT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[0].Bipolis.Transporte.Terrestre, 0, "bipolis");
            costo_parcial_aereo_bipolis[0] = model.AddContainerIncompletoAereoPT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[0].Bipolis.Transporte.Aereo, 0, "bipolis");
            costo_transporte_bipolis[0] = (SilogParams.CostoTransporteTierra1000 * d_semana[0].Bipolis.Transporte.Terrestre + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_terrestre_bipolis[0] * SilogParams.CostoTransporteTierra1000) * SilogParams.PrimaCargaIncompleta) +
                                                    (SilogParams.CostoTransporteAereo1000 * d_semana[0].Bipolis.Transporte.Aereo + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_aereo_bipolis[0] * SilogParams.CostoTransporteAereo1000) * SilogParams.PrimaCargaIncompleta);

            costo_parcial_terrestre_tripolis[0] = model.AddContainerIncompletoTerrestrePT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[0].Tripolis.Transporte.Terrestre, 0, "tripolis");
            costo_parcial_aereo_tripolis[0] = model.AddContainerIncompletoAereoPT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[0].Tripolis.Transporte.Aereo, 0, "tripolis");
            costo_transporte_tripolis[0] = (SilogParams.CostoTransporteTierra1000 * d_semana[0].Tripolis.Transporte.Terrestre + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_terrestre_tripolis[0] * SilogParams.CostoTransporteTierra1000) * SilogParams.PrimaCargaIncompleta) +
                                                     (SilogParams.CostoTransporteAereo1000 * d_semana[0].Tripolis.Transporte.Aereo + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_aereo_tripolis[0] * SilogParams.CostoTransporteAereo1000) * SilogParams.PrimaCargaIncompleta);


            costo_parcial_terrestre_tetrapolis[0] = model.AddContainerIncompletoTerrestrePT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[0].Tetrapolis.Transporte.Terrestre, 0, "tetrapolis");
            costo_parcial_aereo_tetrapolis[0] = model.AddContainerIncompletoAereoPT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[0].Tetrapolis.Transporte.Aereo, 0, "tetrapolis");
            costo_transporte_tetrapolis[0] = (SilogParams.CostoTransporteTierra1400 * d_semana[0].Tetrapolis.Transporte.Terrestre + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_terrestre_tetrapolis[0] * SilogParams.CostoTransporteTierra1400) * SilogParams.PrimaCargaIncompleta) +
                                                       (SilogParams.CostoTransporteAereo1400 * d_semana[0].Tetrapolis.Transporte.Aereo + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_aereo_tetrapolis[0] * SilogParams.CostoTransporteAereo1400) * SilogParams.PrimaCargaIncompleta);


            costo_parcial_terrestre_metropolis[0] = model.AddContainerIncompletoTerrestrePT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[0].Metropolis.Transporte.Terrestre, 0, "metropolis");
            costo_parcial_aereo_metropolis[0] = model.AddContainerIncompletoAereoPT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[0].Metropolis.Transporte.Aereo, 0, "metropolis");
            costo_transporte_metropolis[0] = (SilogParams.CostoTransporteTierra700 * d_semana[0].Metropolis.Transporte.Terrestre + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_terrestre_metropolis[0] * SilogParams.CostoTransporteTierra700) * SilogParams.PrimaCargaIncompleta) +
                                                       (SilogParams.CostoTransporteAereo700 * d_semana[0].Metropolis.Transporte.Aereo + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_aereo_metropolis[0] * SilogParams.CostoTransporteAereo700) * SilogParams.PrimaCargaIncompleta);

            costo_transporte_PT[0] = costo_transporte_bipolis[0] +
                                               costo_transporte_tripolis[0] +
                                               costo_transporte_tetrapolis[0] +
                                               costo_transporte_metropolis[0];

            #endregion

            costos_Transporte[0] = costo_transporte_MP[0] + costo_transporte_PT[0];

            #endregion

            Gastos[0] = costos_Produccion[0] + costos_Transporte[0] +
                                  costos_Almacenamiento[0];

            #endregion

            Utilidad[0] = Ventas[0] - Gastos[0];


            #endregion

            #region Caso Especial Semana 1

            if (numSemanas >= 2)
            {
                #region Produccion

                consumo_alternic[1] = SilogParams.AlternicPorProductoTerminado * d_semana[1].UnidadesAProducir;
                disponible_alternic[1] = disponible_alternic[0] - consumo_alternic[0] + d_semana[0].Alternic.Aereo + parametrosEntrada.MateriasPrimas.Alternic.EnTransito.GetSemana(1);
                model.AddConstraint("s" + 1 + "_consumoMax_alternic", consumo_alternic[1] <= disponible_alternic[1] * SilogParams.ConsumoMaximoDeDisponible);

                consumo_nikelen[1] = SilogParams.NikelenPorProductoTerminado * d_semana[1].UnidadesAProducir;
                disponible_nikelen[1] = disponible_nikelen[0] - consumo_nikelen[0] + d_semana[0].Nikelen.Aereo + parametrosEntrada.MateriasPrimas.Nikelen.EnTransito.GetSemana(1);
                model.AddConstraint("s" + 1 + "_consumoMax_nikelen", consumo_nikelen[1] <= disponible_nikelen[1] * SilogParams.ConsumoMaximoDeDisponible);

                consumo_progesic[1] = SilogParams.ProgesicPorProductoTerminado * d_semana[1].UnidadesAProducir;
                disponible_progesic[1] = disponible_progesic[0] - consumo_progesic[0] + parametrosEntrada.MateriasPrimas.Progesic.EnTransito.GetSemana(1);
                model.AddConstraint("s" + 1 + "_consumoMax_progesic", consumo_progesic[1] <= disponible_progesic[1] * SilogParams.ConsumoMaximoDeDisponible);

                #endregion

                #region Ventas

                salida_Monopolis[1] = (d_semana[1].Bipolis.Transporte.Aereo +
                                                 d_semana[1].Bipolis.Transporte.Terrestre +
                                                 d_semana[1].Tripolis.Transporte.Aereo +
                                                 d_semana[1].Tripolis.Transporte.Terrestre +
                                                 d_semana[1].Tetrapolis.Transporte.Aereo +
                                                 d_semana[1].Tetrapolis.Transporte.Terrestre +
                                                 d_semana[1].Metropolis.Transporte.Aereo +
                                                 d_semana[1].Metropolis.Transporte.Terrestre);

                disponible_Monopolis[1] = disponible_Monopolis[0] - d_semana[0].Monopolis.Ventas + d_semana[0].UnidadesAProducir - salida_Monopolis[1];
                d_semana[1].Monopolis.Ventas = model.AddVentasSemana(1, "monopolis", parametrosEntrada.ProductoTerminado.Monopolis.DemandaEstimada.GetSemana(1), disponible_Monopolis[1]);

                disponible_Bipolis[1] = (disponible_Bipolis[0] - d_semana[0].Bipolis.Ventas) + d_semana[0].Bipolis.Transporte.Aereo + parametrosEntrada.ProductoTerminado.Bipolis.EnTransito.GetSemana(1);
                d_semana[1].Bipolis.Ventas = model.AddVentasSemana(1, "bipolis", parametrosEntrada.ProductoTerminado.Bipolis.DemandaEstimada.GetSemana(1), disponible_Bipolis[1]);

                disponible_Tripolis[1] = (disponible_Tripolis[0] - d_semana[0].Tripolis.Ventas) + d_semana[0].Tripolis.Transporte.Aereo + parametrosEntrada.ProductoTerminado.Tripolis.EnTransito.GetSemana(1);
                d_semana[1].Tripolis.Ventas = model.AddVentasSemana(1, "tripolis", parametrosEntrada.ProductoTerminado.Tripolis.DemandaEstimada.GetSemana(1), disponible_Tripolis[1]);

                disponible_Tetrapolis[1] = (disponible_Tetrapolis[0] - d_semana[0].Tetrapolis.Ventas) + d_semana[0].Tetrapolis.Transporte.Aereo + parametrosEntrada.ProductoTerminado.Tetrapolis.EnTransito.GetSemana(1);
                d_semana[1].Tetrapolis.Ventas = model.AddVentasSemana(1, "tetrapolis", parametrosEntrada.ProductoTerminado.Tetrapolis.DemandaEstimada.GetSemana(1), disponible_Tetrapolis[1]);

                disponible_Metropolis[1] = (disponible_Metropolis[0] - d_semana[0].Metropolis.Ventas) + d_semana[0].Metropolis.Transporte.Aereo + parametrosEntrada.ProductoTerminado.Metropolis.EnTransito.GetSemana(1);
                d_semana[1].Metropolis.Ventas = model.AddVentasSemana(1, "metropolis", parametrosEntrada.ProductoTerminado.Metropolis.DemandaEstimada.GetSemana(1), disponible_Metropolis[1]);

                UnidadesVendidas[1] = d_semana[1].Monopolis.Ventas +
                             d_semana[1].Bipolis.Ventas +
                             d_semana[1].Tripolis.Ventas +
                             d_semana[1].Tetrapolis.Ventas +
                             d_semana[1].Metropolis.Ventas;

                DemandaTotalEstimada[1] =
                 parametrosEntrada.ProductoTerminado.Monopolis.DemandaEstimada.GetSemana(1) +
                 parametrosEntrada.ProductoTerminado.Bipolis.DemandaEstimada.GetSemana(1) +
                 parametrosEntrada.ProductoTerminado.Tripolis.DemandaEstimada.GetSemana(1) +
                 parametrosEntrada.ProductoTerminado.Tetrapolis.DemandaEstimada.GetSemana(1) +
                 parametrosEntrada.ProductoTerminado.Metropolis.DemandaEstimada.GetSemana(1);

                NivelDeServicio[1] = new Decision(Domain.Real, "s1_nivelDeServicio");
                model.AddDecision(NivelDeServicio[1]);
                model.AddConstraint("s1_constraint_nivelDeServicio", NivelDeServicio[1] == UnidadesVendidas[1] / DemandaTotalEstimada[1]);

                if (parametrosEntrada.NivelDeServicioMínimo > 1)
                {
                    model.AddConstraint("s1_nivelDeServicio", UnidadesVendidas[1] >= DemandaTotalEstimada[1] * parametrosEntrada.NivelDeServicioMínimo);
                }

                ventasTotal = ventasTotal + UnidadesVendidas[1];
                demandaTotal = demandaTotal + DemandaTotalEstimada[1];

                Ventas[1] = UnidadesVendidas[1] * SilogParams.PrecioDeVenta;


                #endregion

                #region Gastos

                costos_Produccion[1] = SilogParams.CostoFijoProduccion + SilogParams.CostoVariableProduccion * d_semana[1].UnidadesAProducir +
                                                 consumo_alternic[1] * SilogParams.CostoAcarreoAlmacenPropioMP +
                                                 consumo_nikelen[1] * SilogParams.CostoAcarreoAlmacenPropioMP +
                                                 consumo_progesic[1] * SilogParams.CostoAcarreoAlmacenPropioMP;

                #region Costos Almacen

                capacidad_almacenPropio_MP[1] = capacidad_almacenPropio_MP[0] + parametrosEntrada.MateriasPrimas.CapacidadEnConstruccion;
                capacidad_almacenAlquilado_MP[1] = d_semana[1].AlquilarAlmacen * SilogParams.CapacidadAlmacenAlquilado;

                model.AddConstraint("s" + 1 + "_cantidadAlmancen_alternic", disponible_alternic[1] == d_semana[1].Alternic.AlmacenPropio + d_semana[1].Alternic.AlmacenAlquilado + d_semana[1].Alternic.Detencion);
                model.AddConstraint("s" + 1 + "_cantidadAlmancen_nikelen", disponible_nikelen[1] == d_semana[1].Nikelen.AlmacenPropio + d_semana[1].Nikelen.AlmacenAlquilado + d_semana[1].Nikelen.Detencion);
                model.AddConstraint("s" + 1 + "_cantidadAlmancen_progesic", disponible_progesic[1] == d_semana[1].Progesic.AlmacenPropio + d_semana[1].Progesic.AlmacenAlquilado + d_semana[1].Progesic.Detencion);

                model.AddConstraint("s" + 1 + "_capacidadAlmancenPropio_MP", capacidad_almacenPropio_MP[1] >= d_semana[1].Alternic.AlmacenPropio + d_semana[1].Nikelen.AlmacenPropio + d_semana[1].Progesic.AlmacenPropio);
                model.AddConstraint("s" + 1 + "_capacidadAlmancenAlquilado_MP", capacidad_almacenAlquilado_MP[1] >= d_semana[1].Alternic.AlmacenAlquilado + d_semana[1].Nikelen.AlmacenAlquilado + d_semana[1].Progesic.AlmacenAlquilado);

                costo_almacenamiento_mp[1] =
                    d_semana[1].AgrandarAlmacen * SilogParams.CostoExpansionAlmacenMP +
                    SilogParams.CostoFijoAlmacenPropioMP * capacidad_almacenPropio_MP[1] +
                    //Costo almacenamiento propio
                    (d_semana[1].Alternic.AlmacenPropio * SilogParams.CostoVariableAlmacenPropioMP) +
                    (d_semana[1].Nikelen.AlmacenPropio * SilogParams.CostoVariableAlmacenPropioMP) +
                    (d_semana[1].Progesic.AlmacenPropio * SilogParams.CostoVariableAlmacenPropioMP) +
                    //Costo almacenamiento alquilado
                    d_semana[1].AlquilarAlmacen * SilogParams.CostoFijoAlmacenAlquilado +
                    (d_semana[1].Alternic.AlmacenAlquilado * SilogParams.CostoVariableAlmacenAlquilado) +
                    (d_semana[1].Nikelen.AlmacenAlquilado * SilogParams.CostoVariableAlmacenAlquilado) +
                    (d_semana[1].Progesic.AlmacenAlquilado * SilogParams.CostoVariableAlmacenAlquilado) +
                    //Costo en Detencion
                    (d_semana[1].Alternic.Detencion * SilogParams.CostoDetencionMP) +
                    (d_semana[1].Nikelen.Detencion * SilogParams.CostoDetencionMP) +
                    (d_semana[1].Progesic.Detencion * SilogParams.CostoDetencionMP);


                capacidad_almacen_PT_Monopolis[1] = capacidad_almacen_PT_Monopolis[0] + parametrosEntrada.ProductoTerminado.Monopolis.CapacidadEnConstruccion;
                costo_almacenamiento_Monopolis[1] =
                    d_semana[1].Monopolis.AgrandarAlmacen * SilogParams.CostoExpansionAlmacenPT +
                    d_semana[1].Monopolis.Ventas * SilogParams.CostoAcarreoAlmacenPropioPT +
                    model.AddRestriccionAlmacenProductoTerminado(1, disponible_Monopolis[1], capacidad_almacen_PT_Monopolis[1], d_semana[1].Monopolis, "monopolis");

                capacidad_almacen_PT_Bipolis[1] = capacidad_almacen_PT_Bipolis[0] + parametrosEntrada.ProductoTerminado.Bipolis.CapacidadEnConstruccion;
                costo_almacenamiento_Bipolis[1] =
                    d_semana[1].Bipolis.AgrandarAlmacen * SilogParams.CostoExpansionAlmacenPT +
                    d_semana[1].Bipolis.Ventas * SilogParams.CostoAcarreoAlmacenPropioPT +
                    model.AddRestriccionAlmacenProductoTerminado(1, disponible_Bipolis[1], capacidad_almacen_PT_Bipolis[1], d_semana[1].Bipolis, "bipolis");

                capacidad_almacen_PT_Tripolis[1] = capacidad_almacen_PT_Tripolis[0] + parametrosEntrada.ProductoTerminado.Tripolis.CapacidadEnConstruccion;
                costo_almacenamiento_Tripolis[1] =
                    d_semana[1].Tripolis.AgrandarAlmacen * SilogParams.CostoExpansionAlmacenPT +
                    d_semana[1].Tripolis.Ventas * SilogParams.CostoAcarreoAlmacenPropioPT +
                    model.AddRestriccionAlmacenProductoTerminado(1, disponible_Tripolis[1], capacidad_almacen_PT_Tripolis[1], d_semana[1].Tripolis, "tripolis");

                capacidad_almacen_PT_Tetrapolis[1] = capacidad_almacen_PT_Tetrapolis[0] + parametrosEntrada.ProductoTerminado.Tetrapolis.CapacidadEnConstruccion;
                costo_almacenamiento_Tetrapolis[1] =
                    d_semana[1].Tetrapolis.AgrandarAlmacen * SilogParams.CostoExpansionAlmacenPT +
                    d_semana[1].Tetrapolis.Ventas * SilogParams.CostoAcarreoAlmacenPropioPT +
                    model.AddRestriccionAlmacenProductoTerminado(1, disponible_Tetrapolis[1], capacidad_almacen_PT_Tetrapolis[1], d_semana[1].Tetrapolis, "tetrapolis");

                capacidad_almacen_PT_Metropolis[1] = capacidad_almacen_PT_Metropolis[0] + parametrosEntrada.ProductoTerminado.Metropolis.CapacidadEnConstruccion;
                costo_almacenamiento_Metropolis[1] =
                    d_semana[1].Metropolis.AgrandarAlmacen * SilogParams.CostoExpansionAlmacenPT +
                    d_semana[1].Metropolis.Ventas * SilogParams.CostoAcarreoAlmacenPropioPT +
                    model.AddRestriccionAlmacenProductoTerminado(1, disponible_Metropolis[1], capacidad_almacen_PT_Metropolis[1], d_semana[1].Metropolis, "metropolis");

                costos_Almacenamiento[1] =
                    costo_almacenamiento_mp[1] +
                    costo_almacenamiento_Monopolis[1] +
                    costo_almacenamiento_Bipolis[1] +
                    costo_almacenamiento_Tripolis[1] +
                    costo_almacenamiento_Tetrapolis[1] +
                    costo_almacenamiento_Metropolis[1];

                #endregion

                #region Costos Transporte

                #region Materia Prima

                costo_parcial_terrestre_alternic[1] = model.AddContainerIncompletoTerrestreMP(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[1].Alternic.Terrestre, 1, "alternic");
                costo_parcial_aereo_alternic[1] = model.AddContainerIncompletoAereoMP(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[1].Alternic.Aereo, 1, "alternic");
                costo_transporte_alternic[1] = (SilogParams.CostoTransporteTierraAlternic * d_semana[1].Alternic.Terrestre + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_terrestre_alternic[1] * SilogParams.CostoTransporteTierraAlternic) * SilogParams.PrimaCargaIncompleta) +
                                                         (SilogParams.CostoTransporteAereoAlternic * d_semana[1].Alternic.Aereo + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_aereo_alternic[1] * SilogParams.CostoTransporteAereoAlternic) * SilogParams.PrimaCargaIncompleta);

                costo_parcial_terrestre_nikelen[1] = model.AddContainerIncompletoTerrestreMP(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[1].Nikelen.Terrestre, 1, "nikelen");
                costo_parcial_aereo_nikelen[1] = model.AddContainerIncompletoAereoMP(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[1].Nikelen.Aereo, 1, "nikelen");
                costo_transporte_nikelen[1] = (SilogParams.CostoTransporteTierraNikelen * d_semana[1].Nikelen.Terrestre + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_terrestre_nikelen[1] * SilogParams.CostoTransporteTierraNikelen) * SilogParams.PrimaCargaIncompleta) +
                                                        (SilogParams.CostoTransporteAereoNikelen * d_semana[1].Nikelen.Aereo + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_aereo_nikelen[1] * SilogParams.CostoTransporteAereoNikelen) * SilogParams.PrimaCargaIncompleta);

                costo_parcial_terrestre_progesic[1] = model.AddContainerIncompletoTerrestreMP(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[1].Progesic.Terrestre, 1, "progesic");
                costo_parcial_aereo_progesic[1] = model.AddContainerIncompletoAereoMP(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[1].Progesic.Aereo, 1, "progesic");
                costo_transporte_progesic[1] = (SilogParams.CostoTransporteTierraProgesic * d_semana[1].Progesic.Terrestre + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_terrestre_progesic[1] * SilogParams.CostoTransporteTierraProgesic) * SilogParams.PrimaCargaIncompleta) +
                                                         (SilogParams.CostoTransporteAereoProgesic * d_semana[1].Progesic.Aereo + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_aereo_progesic[1] * SilogParams.CostoTransporteAereoProgesic) * SilogParams.PrimaCargaIncompleta);

                costo_transporte_MP[1] = costo_transporte_alternic[1] +
                                                   costo_transporte_nikelen[1] +
                                                   costo_transporte_progesic[1];

                #endregion


                #region Producto Terminado

                costo_parcial_terrestre_bipolis[1] = model.AddContainerIncompletoTerrestrePT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[1].Bipolis.Transporte.Terrestre, 1, "bipolis");
                costo_parcial_aereo_bipolis[1] = model.AddContainerIncompletoAereoPT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[1].Bipolis.Transporte.Aereo, 1, "bipolis");
                costo_transporte_bipolis[1] = (SilogParams.CostoTransporteTierra1000 * d_semana[1].Bipolis.Transporte.Terrestre + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_terrestre_bipolis[1] * SilogParams.CostoTransporteTierra1000) * SilogParams.PrimaCargaIncompleta) +
                                                        (SilogParams.CostoTransporteAereo1000 * d_semana[1].Bipolis.Transporte.Aereo + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_aereo_bipolis[1] * SilogParams.CostoTransporteAereo1000) * SilogParams.PrimaCargaIncompleta);

                costo_parcial_terrestre_tripolis[1] = model.AddContainerIncompletoTerrestrePT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[1].Tripolis.Transporte.Terrestre, 1, "tripolis");
                costo_parcial_aereo_tripolis[1] = model.AddContainerIncompletoAereoPT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[1].Tripolis.Transporte.Aereo, 1, "tripolis");
                costo_transporte_tripolis[1] = (SilogParams.CostoTransporteTierra1000 * d_semana[1].Tripolis.Transporte.Terrestre + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_terrestre_tripolis[1] * SilogParams.CostoTransporteTierra1000) * SilogParams.PrimaCargaIncompleta) +
                                                         (SilogParams.CostoTransporteAereo1000 * d_semana[1].Tripolis.Transporte.Aereo + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_aereo_tripolis[1] * SilogParams.CostoTransporteAereo1000) * SilogParams.PrimaCargaIncompleta);


                costo_parcial_terrestre_tetrapolis[1] = model.AddContainerIncompletoTerrestrePT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[1].Tetrapolis.Transporte.Terrestre, 1, "tetrapolis");
                costo_parcial_aereo_tetrapolis[1] = model.AddContainerIncompletoAereoPT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[1].Tetrapolis.Transporte.Aereo, 1, "tetrapolis");
                costo_transporte_tetrapolis[1] = (SilogParams.CostoTransporteTierra1400 * d_semana[1].Tetrapolis.Transporte.Terrestre + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_terrestre_tetrapolis[1] * SilogParams.CostoTransporteTierra1400) * SilogParams.PrimaCargaIncompleta) +
                                                           (SilogParams.CostoTransporteAereo1400 * d_semana[1].Tetrapolis.Transporte.Aereo + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_aereo_tetrapolis[1] * SilogParams.CostoTransporteAereo1400) * SilogParams.PrimaCargaIncompleta);


                costo_parcial_terrestre_metropolis[1] = model.AddContainerIncompletoTerrestrePT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[1].Metropolis.Transporte.Terrestre, 1, "metropolis");
                costo_parcial_aereo_metropolis[1] = model.AddContainerIncompletoAereoPT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[1].Metropolis.Transporte.Aereo, 1, "metropolis");
                costo_transporte_metropolis[1] = (SilogParams.CostoTransporteTierra700 * d_semana[1].Metropolis.Transporte.Terrestre + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_terrestre_metropolis[1] * SilogParams.CostoTransporteTierra700) * SilogParams.PrimaCargaIncompleta) +
                                                           (SilogParams.CostoTransporteAereo700 * d_semana[1].Metropolis.Transporte.Aereo + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_aereo_metropolis[1] * SilogParams.CostoTransporteAereo700) * SilogParams.PrimaCargaIncompleta);

                costo_transporte_PT[1] = costo_transporte_bipolis[1] +
                                                   costo_transporte_tripolis[1] +
                                                   costo_transporte_tetrapolis[1] +
                                                   costo_transporte_metropolis[1];

                #endregion

                costos_Transporte[1] = costo_transporte_MP[1] + costo_transporte_PT[1];

                #endregion

                Gastos[1] = costos_Produccion[1] + costos_Transporte[1] +
                                      costos_Almacenamiento[1];

                #endregion

                Utilidad[1] = Ventas[1] - Gastos[1];
            }

            #endregion

            for (int currentWeek = 2; currentWeek <= numSemanas - 1; currentWeek++)
            {
                #region Produccion

                consumo_alternic[currentWeek] = SilogParams.AlternicPorProductoTerminado * d_semana[currentWeek].UnidadesAProducir;
                disponible_alternic[currentWeek] = disponible_alternic[currentWeek - 1] - consumo_alternic[currentWeek - 1] + d_semana[currentWeek - 1].Alternic.Aereo + d_semana[currentWeek - 3].Alternic.Terrestre + parametrosEntrada.MateriasPrimas.Alternic.EnTransito.GetSemana(currentWeek);
                model.AddConstraint("s" + currentWeek + "_consumoMax_alternic", consumo_alternic[currentWeek] <= disponible_alternic[currentWeek] * SilogParams.ConsumoMaximoDeDisponible);

                consumo_nikelen[currentWeek] = SilogParams.NikelenPorProductoTerminado * d_semana[currentWeek].UnidadesAProducir;
                disponible_nikelen[currentWeek] = disponible_nikelen[currentWeek - 1] - consumo_nikelen[currentWeek - 1] + d_semana[currentWeek - 1].Nikelen.Aereo + d_semana[currentWeek - 2].Nikelen.Terrestre + parametrosEntrada.MateriasPrimas.Nikelen.EnTransito.GetSemana(currentWeek);
                model.AddConstraint("s" + currentWeek + "_consumoMax_nikelen", consumo_nikelen[currentWeek] <= disponible_nikelen[currentWeek] * SilogParams.ConsumoMaximoDeDisponible);

                consumo_progesic[currentWeek] = SilogParams.ProgesicPorProductoTerminado * d_semana[currentWeek].UnidadesAProducir;
                disponible_progesic[currentWeek] = disponible_progesic[currentWeek - 1] - consumo_progesic[currentWeek - 1] + d_semana[currentWeek - 2].Progesic.Aereo + d_semana[currentWeek - 5].Progesic.Terrestre + parametrosEntrada.MateriasPrimas.Progesic.EnTransito.GetSemana(currentWeek);
                model.AddConstraint("s" + currentWeek + "_consumoMax_progesic", consumo_progesic[currentWeek] <= disponible_progesic[currentWeek] * SilogParams.ConsumoMaximoDeDisponible);

                #endregion

                #region Ventas

                salida_Monopolis[currentWeek] = (d_semana[currentWeek].Bipolis.Transporte.Aereo +
                                                 d_semana[currentWeek].Bipolis.Transporte.Terrestre +
                                                 d_semana[currentWeek].Tripolis.Transporte.Aereo +
                                                 d_semana[currentWeek].Tripolis.Transporte.Terrestre +
                                                 d_semana[currentWeek].Tetrapolis.Transporte.Aereo +
                                                 d_semana[currentWeek].Tetrapolis.Transporte.Terrestre +
                                                 d_semana[currentWeek].Metropolis.Transporte.Aereo +
                                                 d_semana[currentWeek].Metropolis.Transporte.Terrestre);

                disponible_Monopolis[currentWeek] = disponible_Monopolis[currentWeek - 1] - d_semana[currentWeek - 1].Monopolis.Ventas + d_semana[currentWeek - 1].UnidadesAProducir - salida_Monopolis[currentWeek];
                d_semana[currentWeek].Monopolis.Ventas = model.AddVentasSemana(currentWeek, "monopolis", parametrosEntrada.ProductoTerminado.Monopolis.DemandaEstimada.GetSemana(currentWeek), disponible_Monopolis[currentWeek]);

                disponible_Bipolis[currentWeek] = (disponible_Bipolis[currentWeek - 1] - d_semana[currentWeek - 1].Bipolis.Ventas) + d_semana[currentWeek - 1].Bipolis.Transporte.Aereo + d_semana[currentWeek - 3].Bipolis.Transporte.Terrestre + parametrosEntrada.ProductoTerminado.Bipolis.EnTransito.GetSemana(currentWeek);
                d_semana[currentWeek].Bipolis.Ventas = model.AddVentasSemana(currentWeek, "bipolis", parametrosEntrada.ProductoTerminado.Bipolis.DemandaEstimada.GetSemana(currentWeek), disponible_Bipolis[currentWeek]);

                disponible_Tripolis[currentWeek] = (disponible_Tripolis[currentWeek - 1] - d_semana[currentWeek - 1].Tripolis.Ventas) + d_semana[currentWeek - 1].Tripolis.Transporte.Aereo + d_semana[currentWeek - 3].Tripolis.Transporte.Terrestre + parametrosEntrada.ProductoTerminado.Tripolis.EnTransito.GetSemana(currentWeek);
                d_semana[currentWeek].Tripolis.Ventas = model.AddVentasSemana(currentWeek, "tripolis", parametrosEntrada.ProductoTerminado.Tripolis.DemandaEstimada.GetSemana(currentWeek), disponible_Tripolis[currentWeek]);

                disponible_Tetrapolis[currentWeek] = (disponible_Tetrapolis[currentWeek - 1] - d_semana[currentWeek - 1].Tetrapolis.Ventas) + d_semana[currentWeek - 1].Tetrapolis.Transporte.Aereo + d_semana[currentWeek - 4].Tetrapolis.Transporte.Terrestre + parametrosEntrada.ProductoTerminado.Tetrapolis.EnTransito.GetSemana(currentWeek);
                d_semana[currentWeek].Tetrapolis.Ventas = model.AddVentasSemana(currentWeek, "tetrapolis", parametrosEntrada.ProductoTerminado.Tetrapolis.DemandaEstimada.GetSemana(currentWeek), disponible_Tetrapolis[currentWeek]);

                disponible_Metropolis[currentWeek] = (disponible_Metropolis[currentWeek - 1] - d_semana[currentWeek - 1].Metropolis.Ventas) + d_semana[currentWeek - 1].Metropolis.Transporte.Aereo + d_semana[currentWeek - 2].Metropolis.Transporte.Terrestre + parametrosEntrada.ProductoTerminado.Metropolis.EnTransito.GetSemana(currentWeek);
                d_semana[currentWeek].Metropolis.Ventas = model.AddVentasSemana(currentWeek, "metropolis", parametrosEntrada.ProductoTerminado.Metropolis.DemandaEstimada.GetSemana(currentWeek), disponible_Metropolis[currentWeek]);

                UnidadesVendidas[currentWeek] = d_semana[currentWeek].Monopolis.Ventas +
                    d_semana[currentWeek].Bipolis.Ventas +
                    d_semana[currentWeek].Tripolis.Ventas +
                    d_semana[currentWeek].Tetrapolis.Ventas +
                    d_semana[currentWeek].Metropolis.Ventas;

                DemandaTotalEstimada[currentWeek] =
                    parametrosEntrada.ProductoTerminado.Monopolis.DemandaEstimada.GetSemana(currentWeek) +
                    parametrosEntrada.ProductoTerminado.Bipolis.DemandaEstimada.GetSemana(currentWeek) +
                    parametrosEntrada.ProductoTerminado.Tripolis.DemandaEstimada.GetSemana(currentWeek) +
                    parametrosEntrada.ProductoTerminado.Tetrapolis.DemandaEstimada.GetSemana(currentWeek) +
                    parametrosEntrada.ProductoTerminado.Metropolis.DemandaEstimada.GetSemana(currentWeek);

                var nivelDeServicio = new Decision(Domain.Real, "s" + currentWeek + "_nivelDeServicio");
                model.AddDecision(nivelDeServicio);
                model.AddConstraint("s" + currentWeek + "_constraint_nivelDeServicio", nivelDeServicio == UnidadesVendidas[currentWeek] / DemandaTotalEstimada[currentWeek]);

                if (parametrosEntrada.NivelDeServicioMínimo > 0)
                {
                    model.AddConstraint("s" + currentWeek + "_min_nivelDeServicio", UnidadesVendidas[currentWeek] >= DemandaTotalEstimada[currentWeek] * parametrosEntrada.NivelDeServicioMínimo);
                }


                ventasTotal = ventasTotal + UnidadesVendidas[currentWeek];
                demandaTotal = demandaTotal + DemandaTotalEstimada[currentWeek];

                Ventas[currentWeek] = UnidadesVendidas[currentWeek] * SilogParams.PrecioDeVenta;

                #endregion

                #region Gastos

                costos_Produccion[currentWeek] = SilogParams.CostoFijoProduccion + SilogParams.CostoVariableProduccion * d_semana[currentWeek].UnidadesAProducir +
                                                 consumo_alternic[currentWeek] * SilogParams.CostoAcarreoAlmacenPropioMP +
                                                 consumo_nikelen[currentWeek] * SilogParams.CostoAcarreoAlmacenPropioMP +
                                                 consumo_progesic[currentWeek] * SilogParams.CostoAcarreoAlmacenPropioMP;

                #region Costos Almacen

                capacidad_almacenPropio_MP[currentWeek] = capacidad_almacenPropio_MP[currentWeek - 1] + d_semana[currentWeek - 2].AgrandarAlmacen * SilogParams.CapacidadAdicionalExpansionMP;
                capacidad_almacenAlquilado_MP[currentWeek] = d_semana[currentWeek].AlquilarAlmacen * SilogParams.CapacidadAlmacenAlquilado;

                model.AddConstraint("s" + currentWeek + "_cantidadAlmancen_alternic", disponible_alternic[currentWeek] == d_semana[currentWeek].Alternic.AlmacenPropio + d_semana[currentWeek].Alternic.AlmacenAlquilado + d_semana[currentWeek].Alternic.Detencion);
                model.AddConstraint("s" + currentWeek + "_cantidadAlmancen_nikelen", disponible_nikelen[currentWeek] == d_semana[currentWeek].Nikelen.AlmacenPropio + d_semana[currentWeek].Nikelen.AlmacenAlquilado + d_semana[currentWeek].Nikelen.Detencion);
                model.AddConstraint("s" + currentWeek + "_cantidadAlmancen_progesic", disponible_progesic[currentWeek] == d_semana[currentWeek].Progesic.AlmacenPropio + d_semana[currentWeek].Progesic.AlmacenAlquilado + d_semana[currentWeek].Progesic.Detencion);

                model.AddConstraint("s" + currentWeek + "_capacidadAlmancenPropio_MP", capacidad_almacenPropio_MP[currentWeek] >= d_semana[currentWeek].Alternic.AlmacenPropio + d_semana[currentWeek].Nikelen.AlmacenPropio + d_semana[currentWeek].Progesic.AlmacenPropio);
                model.AddConstraint("s" + currentWeek + "_capacidadAlmancenAlquilado_MP", capacidad_almacenAlquilado_MP[currentWeek] >= d_semana[currentWeek].Alternic.AlmacenAlquilado + d_semana[currentWeek].Nikelen.AlmacenAlquilado + d_semana[currentWeek].Progesic.AlmacenAlquilado);

                costo_almacenamiento_mp[currentWeek] =
                    d_semana[currentWeek].AgrandarAlmacen * SilogParams.CostoExpansionAlmacenMP +
                    SilogParams.CostoFijoAlmacenPropioMP * capacidad_almacenPropio_MP[currentWeek] +
                    //Costo almacenamiento propio
                    (d_semana[currentWeek].Alternic.AlmacenPropio * SilogParams.CostoVariableAlmacenPropioMP) +
                    (d_semana[currentWeek].Nikelen.AlmacenPropio * SilogParams.CostoVariableAlmacenPropioMP) +
                    (d_semana[currentWeek].Progesic.AlmacenPropio * SilogParams.CostoVariableAlmacenPropioMP) +
                    //Costo almacenamiento alquilado
                    d_semana[currentWeek].AlquilarAlmacen * SilogParams.CostoFijoAlmacenAlquilado +
                    (d_semana[currentWeek].Alternic.AlmacenAlquilado * SilogParams.CostoVariableAlmacenAlquilado) +
                    (d_semana[currentWeek].Nikelen.AlmacenAlquilado * SilogParams.CostoVariableAlmacenAlquilado) +
                    (d_semana[currentWeek].Progesic.AlmacenAlquilado * SilogParams.CostoVariableAlmacenAlquilado) +
                    //Costo en Detencion
                    (d_semana[currentWeek].Alternic.Detencion * SilogParams.CostoDetencionMP) +
                    (d_semana[currentWeek].Nikelen.Detencion * SilogParams.CostoDetencionMP) +
                    (d_semana[currentWeek].Progesic.Detencion * SilogParams.CostoDetencionMP);


                capacidad_almacen_PT_Monopolis[currentWeek] = capacidad_almacen_PT_Monopolis[currentWeek - 1] + d_semana[currentWeek - 2].Monopolis.AgrandarAlmacen * SilogParams.CapacidadAdicionalExpansionPT;
                costo_almacenamiento_Monopolis[currentWeek] =
                    d_semana[currentWeek].Monopolis.AgrandarAlmacen * SilogParams.CostoExpansionAlmacenPT +
                    d_semana[currentWeek].Monopolis.Ventas * SilogParams.CostoAcarreoAlmacenPropioPT +
                    model.AddRestriccionAlmacenProductoTerminado(currentWeek, disponible_Monopolis[currentWeek], capacidad_almacen_PT_Monopolis[currentWeek], d_semana[currentWeek].Monopolis, "monopolis");

                capacidad_almacen_PT_Bipolis[currentWeek] = capacidad_almacen_PT_Bipolis[currentWeek - 1] + d_semana[currentWeek - 2].Bipolis.AgrandarAlmacen * SilogParams.CapacidadAdicionalExpansionPT;
                costo_almacenamiento_Bipolis[currentWeek] =
                    d_semana[currentWeek].Bipolis.AgrandarAlmacen * SilogParams.CostoExpansionAlmacenPT +
                    d_semana[currentWeek].Bipolis.Ventas * SilogParams.CostoAcarreoAlmacenPropioPT +
                    model.AddRestriccionAlmacenProductoTerminado(currentWeek, disponible_Bipolis[currentWeek], capacidad_almacen_PT_Bipolis[currentWeek], d_semana[currentWeek].Bipolis, "bipolis");

                capacidad_almacen_PT_Tripolis[currentWeek] = capacidad_almacen_PT_Tripolis[currentWeek - 1] + d_semana[currentWeek - 2].Tripolis.AgrandarAlmacen * SilogParams.CapacidadAdicionalExpansionPT;
                costo_almacenamiento_Tripolis[currentWeek] =
                    d_semana[currentWeek].Tripolis.AgrandarAlmacen * SilogParams.CostoExpansionAlmacenPT +
                    d_semana[currentWeek].Tripolis.Ventas * SilogParams.CostoAcarreoAlmacenPropioPT +
                    model.AddRestriccionAlmacenProductoTerminado(currentWeek, disponible_Tripolis[currentWeek], capacidad_almacen_PT_Tripolis[currentWeek], d_semana[currentWeek].Tripolis, "tripolis");

                capacidad_almacen_PT_Tetrapolis[currentWeek] = capacidad_almacen_PT_Tetrapolis[currentWeek - 1] + d_semana[currentWeek - 2].Tetrapolis.AgrandarAlmacen * SilogParams.CapacidadAdicionalExpansionPT;
                costo_almacenamiento_Tetrapolis[currentWeek] =
                    d_semana[currentWeek].Tetrapolis.AgrandarAlmacen * SilogParams.CostoExpansionAlmacenPT +
                    d_semana[currentWeek].Tetrapolis.Ventas * SilogParams.CostoAcarreoAlmacenPropioPT +
                    model.AddRestriccionAlmacenProductoTerminado(currentWeek, disponible_Tetrapolis[currentWeek], capacidad_almacen_PT_Tetrapolis[currentWeek], d_semana[currentWeek].Tetrapolis, "tetrapolis");

                capacidad_almacen_PT_Metropolis[currentWeek] = capacidad_almacen_PT_Metropolis[currentWeek - 1] + d_semana[currentWeek - 2].Metropolis.AgrandarAlmacen * SilogParams.CapacidadAdicionalExpansionPT;
                costo_almacenamiento_Metropolis[currentWeek] =
                    d_semana[currentWeek].Metropolis.AgrandarAlmacen * SilogParams.CostoExpansionAlmacenPT +
                    d_semana[currentWeek].Metropolis.Ventas * SilogParams.CostoAcarreoAlmacenPropioPT +
                    model.AddRestriccionAlmacenProductoTerminado(currentWeek, disponible_Metropolis[currentWeek], capacidad_almacen_PT_Metropolis[currentWeek], d_semana[currentWeek].Metropolis, "metropolis");

                costos_Almacenamiento[currentWeek] =
                    costo_almacenamiento_mp[currentWeek] +
                    costo_almacenamiento_Monopolis[currentWeek] +
                    costo_almacenamiento_Bipolis[currentWeek] +
                    costo_almacenamiento_Tripolis[currentWeek] +
                    costo_almacenamiento_Tetrapolis[currentWeek] +
                    costo_almacenamiento_Metropolis[currentWeek];

                #endregion

                #region Costos Transporte

                #region Materia Prima

                costo_parcial_terrestre_alternic[currentWeek] = model.AddContainerIncompletoTerrestreMP(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[currentWeek].Alternic.Terrestre, currentWeek, "alternic");
                costo_parcial_aereo_alternic[currentWeek] = model.AddContainerIncompletoAereoMP(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[currentWeek].Alternic.Aereo, currentWeek, "alternic");
                costo_transporte_alternic[currentWeek] = (SilogParams.CostoTransporteTierraAlternic * d_semana[currentWeek].Alternic.Terrestre + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_terrestre_alternic[currentWeek] * SilogParams.CostoTransporteTierraAlternic) * SilogParams.PrimaCargaIncompleta) +
                                                         (SilogParams.CostoTransporteAereoAlternic * d_semana[currentWeek].Alternic.Aereo + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_aereo_alternic[currentWeek] * SilogParams.CostoTransporteAereoAlternic) * SilogParams.PrimaCargaIncompleta);

                costo_parcial_terrestre_nikelen[currentWeek] = model.AddContainerIncompletoTerrestreMP(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[currentWeek].Nikelen.Terrestre, currentWeek, "nikelen");
                costo_parcial_aereo_nikelen[currentWeek] = model.AddContainerIncompletoAereoMP(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[currentWeek].Nikelen.Aereo, currentWeek, "nikelen");
                costo_transporte_nikelen[currentWeek] = (SilogParams.CostoTransporteTierraNikelen * d_semana[currentWeek].Nikelen.Terrestre + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_terrestre_nikelen[currentWeek] * SilogParams.CostoTransporteTierraNikelen) * SilogParams.PrimaCargaIncompleta) +
                                                        (SilogParams.CostoTransporteAereoNikelen * d_semana[currentWeek].Nikelen.Aereo + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_aereo_nikelen[currentWeek] * SilogParams.CostoTransporteAereoNikelen) * SilogParams.PrimaCargaIncompleta);

                costo_parcial_terrestre_progesic[currentWeek] = model.AddContainerIncompletoTerrestreMP(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[currentWeek].Progesic.Terrestre, currentWeek, "progesic");
                costo_parcial_aereo_progesic[currentWeek] = model.AddContainerIncompletoAereoMP(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[currentWeek].Progesic.Aereo, currentWeek, "progesic");
                costo_transporte_progesic[currentWeek] = (SilogParams.CostoTransporteTierraProgesic * d_semana[currentWeek].Progesic.Terrestre + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_terrestre_progesic[currentWeek] * SilogParams.CostoTransporteTierraProgesic) * SilogParams.PrimaCargaIncompleta) +
                                                         (SilogParams.CostoTransporteAereoProgesic * d_semana[currentWeek].Progesic.Aereo + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_aereo_progesic[currentWeek] * SilogParams.CostoTransporteAereoProgesic) * SilogParams.PrimaCargaIncompleta);

                costo_transporte_MP[currentWeek] = costo_transporte_alternic[currentWeek] +
                                                   costo_transporte_nikelen[currentWeek] +
                                                   costo_transporte_progesic[currentWeek];

                #endregion


                #region Producto Terminado

                costo_parcial_terrestre_bipolis[currentWeek] = model.AddContainerIncompletoTerrestrePT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[currentWeek].Bipolis.Transporte.Terrestre, currentWeek, "bipolis");
                costo_parcial_aereo_bipolis[currentWeek] = model.AddContainerIncompletoAereoPT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[currentWeek].Bipolis.Transporte.Aereo, currentWeek, "bipolis");
                costo_transporte_bipolis[currentWeek] = (SilogParams.CostoTransporteTierra1000 * d_semana[currentWeek].Bipolis.Transporte.Terrestre + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_terrestre_bipolis[currentWeek] * SilogParams.CostoTransporteTierra1000) * SilogParams.PrimaCargaIncompleta) +
                                                        (SilogParams.CostoTransporteAereo1000 * d_semana[currentWeek].Bipolis.Transporte.Aereo + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_aereo_bipolis[currentWeek] * SilogParams.CostoTransporteAereo1000) * SilogParams.PrimaCargaIncompleta);

                costo_parcial_terrestre_tripolis[currentWeek] = model.AddContainerIncompletoTerrestrePT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[currentWeek].Tripolis.Transporte.Terrestre, currentWeek, "tripolis");
                costo_parcial_aereo_tripolis[currentWeek] = model.AddContainerIncompletoAereoPT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[currentWeek].Tripolis.Transporte.Aereo, currentWeek, "tripolis");
                costo_transporte_tripolis[currentWeek] = (SilogParams.CostoTransporteTierra1000 * d_semana[currentWeek].Tripolis.Transporte.Terrestre + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_terrestre_tripolis[currentWeek] * SilogParams.CostoTransporteTierra1000) * SilogParams.PrimaCargaIncompleta) +
                                                         (SilogParams.CostoTransporteAereo1000 * d_semana[currentWeek].Tripolis.Transporte.Aereo + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_aereo_tripolis[currentWeek] * SilogParams.CostoTransporteAereo1000) * SilogParams.PrimaCargaIncompleta);


                costo_parcial_terrestre_tetrapolis[currentWeek] = model.AddContainerIncompletoTerrestrePT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[currentWeek].Tetrapolis.Transporte.Terrestre, currentWeek, "tetrapolis");
                costo_parcial_aereo_tetrapolis[currentWeek] = model.AddContainerIncompletoAereoPT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[currentWeek].Tetrapolis.Transporte.Aereo, currentWeek, "tetrapolis");
                costo_transporte_tetrapolis[currentWeek] = (SilogParams.CostoTransporteTierra1400 * d_semana[currentWeek].Tetrapolis.Transporte.Terrestre + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_terrestre_tetrapolis[currentWeek] * SilogParams.CostoTransporteTierra1400) * SilogParams.PrimaCargaIncompleta) +
                                                           (SilogParams.CostoTransporteAereo1400 * d_semana[currentWeek].Tetrapolis.Transporte.Aereo + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_aereo_tetrapolis[currentWeek] * SilogParams.CostoTransporteAereo1400) * SilogParams.PrimaCargaIncompleta);


                costo_parcial_terrestre_metropolis[currentWeek] = model.AddContainerIncompletoTerrestrePT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[currentWeek].Metropolis.Transporte.Terrestre, currentWeek, "metropolis");
                costo_parcial_aereo_metropolis[currentWeek] = model.AddContainerIncompletoAereoPT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, d_semana[currentWeek].Metropolis.Transporte.Aereo, currentWeek, "metropolis");
                costo_transporte_metropolis[currentWeek] = (SilogParams.CostoTransporteTierra700 * d_semana[currentWeek].Metropolis.Transporte.Terrestre + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_terrestre_metropolis[currentWeek] * SilogParams.CostoTransporteTierra700) * SilogParams.PrimaCargaIncompleta) +
                                                           (SilogParams.CostoTransporteAereo700 * d_semana[currentWeek].Metropolis.Transporte.Aereo + SilogParams.CostoOrdenamientoMP) + ((costo_parcial_aereo_metropolis[currentWeek] * SilogParams.CostoTransporteAereo700) * SilogParams.PrimaCargaIncompleta);

                costo_transporte_PT[currentWeek] = costo_transporte_bipolis[currentWeek] +
                                                   costo_transporte_tripolis[currentWeek] +
                                                   costo_transporte_tetrapolis[currentWeek] +
                                                   costo_transporte_metropolis[currentWeek];

                #endregion

                costos_Transporte[currentWeek] = costo_transporte_MP[currentWeek] + costo_transporte_PT[currentWeek];

                #endregion

                Gastos[currentWeek] = costos_Produccion[currentWeek] + costos_Transporte[currentWeek] +
                                      costos_Almacenamiento[currentWeek];

                #endregion

                Utilidad[currentWeek] = Ventas[currentWeek] - Gastos[currentWeek];
            }

            var nivelDeServicioAcumulado = new Decision(Domain.Real, "totales_nivelDeServicio");
            model.AddDecision(nivelDeServicioAcumulado);

            model.AddConstraint("totales_const_nivelDeServicio", nivelDeServicioAcumulado == ventasTotal / demandaTotal);


            #region semana N+1

            disponible_alternic[numSemanas] = disponible_alternic[numSemanas - 1] - consumo_alternic[numSemanas - 1] + d_semana[numSemanas - 1].Alternic.Aereo + d_semana[numSemanas - 3].Alternic.Terrestre;
            model.AddConstraint("s" + numSemanas + "_consumoMax_alternic", disponible_alternic[numSemanas] >= 24000);

            disponible_nikelen[numSemanas] = disponible_nikelen[numSemanas - 1] - consumo_nikelen[numSemanas - 1] + d_semana[numSemanas - 1].Nikelen.Aereo + d_semana[numSemanas - 2].Nikelen.Terrestre;
            model.AddConstraint("s" + numSemanas + "_consumoMax_nikelen", disponible_nikelen[numSemanas] >= 16000);

            disponible_progesic[numSemanas] = disponible_progesic[numSemanas - 1] - consumo_progesic[numSemanas - 1] + d_semana[numSemanas - 2].Progesic.Aereo + d_semana[numSemanas - 5].Progesic.Terrestre;
            model.AddConstraint("s" + numSemanas + "_consumoMax_progesic", disponible_progesic[numSemanas] >= 8000);

            Term disponibilidadFinal =
                //Disponible Monopolis
                disponible_Monopolis[numSemanas - 1] - d_semana[numSemanas - 1].Monopolis.Ventas + d_semana[numSemanas - 1].UnidadesAProducir +

                //Disponible Bipolis
                (disponible_Bipolis[numSemanas - 1] - d_semana[numSemanas - 1].Bipolis.Ventas) + d_semana[numSemanas - 1].Bipolis.Transporte.Aereo + d_semana[numSemanas - 3].Bipolis.Transporte.Terrestre +
                //En transitoBipolis
                d_semana[numSemanas - 2].Bipolis.Transporte.Terrestre + d_semana[numSemanas - 1].Bipolis.Transporte.Terrestre +

                //Disponible Tripolis
                (disponible_Tripolis[numSemanas - 1] - d_semana[numSemanas - 1].Tripolis.Ventas) + d_semana[numSemanas - 1].Tripolis.Transporte.Aereo + d_semana[numSemanas - 3].Tripolis.Transporte.Terrestre +
                //En transitoTripolis
                d_semana[numSemanas - 2].Tripolis.Transporte.Terrestre + d_semana[numSemanas - 1].Tripolis.Transporte.Terrestre +

                //Disponible Tetrapolis
                (disponible_Tetrapolis[numSemanas - 1] - d_semana[numSemanas - 1].Tetrapolis.Ventas) + d_semana[numSemanas - 1].Tetrapolis.Transporte.Aereo + d_semana[numSemanas - 4].Tetrapolis.Transporte.Terrestre +
                //En transitoTetrapolis
                d_semana[numSemanas - 3].Tetrapolis.Transporte.Terrestre + d_semana[numSemanas - 2].Tetrapolis.Transporte.Terrestre + d_semana[numSemanas - 1].Tetrapolis.Transporte.Terrestre +


                //Disponible Metropolis
                (disponible_Metropolis[numSemanas - 1] - d_semana[numSemanas - 1].Metropolis.Ventas) + d_semana[numSemanas - 1].Metropolis.Transporte.Aereo + d_semana[numSemanas - 2].Metropolis.Transporte.Terrestre +
                //En transitoMetropolis
                d_semana[numSemanas - 1].Metropolis.Transporte.Terrestre;


            model.AddConstraint("disponiblePT_Final", disponibilidadFinal >= 5000);

            #endregion


            for (var i = 0; i < Utilidad.Length; i++)
            {
                var dec = new Decision(Domain.Real, "s" + i + "_Utilidad");
                model.AddDecision(dec);
                model.AddConstraint("s" + i + "_const_utilidad", dec == Utilidad[i]);
            }


            var UtilidadTerm = Utilidad.Aggregate((Term)0, (term, term1) => term + term1);

            model.AddGoal("maximizarUtilidad", GoalKind.Maximize, UtilidadTerm);

            try
            {
                Solution solution = context.Solve();
                Report report = solution.GetReport(ReportVerbosity.SolverDetails);

                var ordered = solution.Decisions.GroupBy(c => c.Name.Substring(0, c.Name.IndexOf('_')));

                Console.Write("{0}", report);

                foreach (var t in ordered)
                {
                    Console.WriteLine(("".PadLeft(9, '=') + t.Key).PadRight(20, '='));

                    var subG = t.GroupBy(c => GetNextIdentifier(c.Name, t.Key.Length + 1));

                    foreach (var subGG in subG)
                    {
                        Console.WriteLine(("".PadLeft(9, '-') + subGG.Key).PadRight(20, '-'));

                        foreach (var dec in subGG.OrderBy(c => c.Name))
                        {
                            Console.WriteLine(string.Format("{0}: {1}", dec.Name, Math.Round(dec.ToDouble(), 3)));
                        }
                        Console.WriteLine();

                    }

                    Console.WriteLine();
                    Console.WriteLine();


                }



            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException.Message);
            }

            Console.ReadLine();
        }


        private static string GetNextIdentifier(string id, int prefixLength)
        {
            var f = id.Substring(prefixLength);

            if (!f.Contains("_"))
                return f;

            return f.Substring(0, f.IndexOf('_'));
        }

    }



    #region Decisiones

    public class DecisionesSemanales : IDecisionesSemanales
    {
        public DecisionesSemanales(Model model, string prefix)
        {
            AgrandarAlmacen = new Decision(Domain.Boolean, prefix + "_agrandarAlmacen");
            AlquilarAlmacen = new Decision(Domain.Boolean, prefix + "_alquilarAlmacen");
            UnidadesAProducir = new Decision(Domain.IntegerNonnegative, prefix + "_unidadesAProducir");

            model.AddDecisions((Decision)AgrandarAlmacen, (Decision)AlquilarAlmacen, (Decision)UnidadesAProducir);

            Nikelen = new DecisionesMovimiento(model, prefix + "_nikelen");
            Alternic = new DecisionesMovimiento(model, prefix + "_alternic");
            Progesic = new DecisionesMovimiento(model, prefix + "_progesic");

            Monopolis = new DecisionesPTCiudad(model, prefix + "_monopolis");
            Bipolis = new DecisionesPTCiudad(model, prefix + "_bipolis");
            Tripolis = new DecisionesPTCiudad(model, prefix + "_tripolis");
            Tetrapolis = new DecisionesPTCiudad(model, prefix + "_tetrapolis");
            Metropolis = new DecisionesPTCiudad(model, prefix + "_metropolis");


            model.AddConstraint(prefix + "_constraint_unidadesAProducir", UnidadesAProducir <= 3370);
        }
        public Term AgrandarAlmacen { get; private set; }
        public Term AlquilarAlmacen { get; private set; }
        public Term UnidadesAProducir { get; set; }

        public IDecisionesMovimiento Nikelen { get; private set; }
        public IDecisionesMovimiento Progesic { get; private set; }
        public IDecisionesMovimiento Alternic { get; private set; }

        public IDecisionesPTCiudad Monopolis { get; private set; }
        public IDecisionesPTCiudad Bipolis { get; private set; }
        public IDecisionesPTCiudad Tripolis { get; private set; }
        public IDecisionesPTCiudad Tetrapolis { get; private set; }
        public IDecisionesPTCiudad Metropolis { get; private set; }
    }

    public class DecisionesMovimiento : IDecisionesMovimiento
    {
        public DecisionesMovimiento(Model model, string prefix)
        {
            Aereo = new Decision(Domain.IntegerNonnegative, prefix + "_transporte_Aereo");
            Terrestre = new Decision(Domain.IntegerNonnegative, prefix + "_transporte_Terrestre");

            AlmacenPropio = new Decision(Domain.IntegerNonnegative, prefix + "_AlmacenPropio");
            AlmacenAlquilado = new Decision(Domain.IntegerNonnegative, prefix + "_AlmacenAlquilado");
            Detencion = new Decision(Domain.IntegerNonnegative, prefix + "_Detencion");

            model.AddDecisions((Decision)Aereo, (Decision)Terrestre, (Decision)AlmacenPropio, (Decision)AlmacenAlquilado, (Decision)Detencion);
        }
        public Term Aereo { get; private set; }
        public Term Terrestre { get; private set; }

        public Term AlmacenPropio { get; private set; }
        public Term AlmacenAlquilado { get; private set; }
        public Term Detencion { get; private set; }

    }

    public class DecisionesPTCiudad : IDecisionesPTCiudad
    {
        public DecisionesPTCiudad(Model model, string prefix)
        {
            AgrandarAlmacen = new Decision(Domain.Boolean, prefix + "_agrandarAlmacen");

            AlmacenamientoPropio = new Decision(Domain.IntegerNonnegative, prefix + "_almacenPropio");
            Detencion = new Decision(Domain.IntegerNonnegative, prefix + "_detencion");


            model.AddDecisions((Decision)AgrandarAlmacen, (Decision)AlmacenamientoPropio, (Decision)Detencion);

            Transporte = new DecisionesMovimiento(model, prefix);
        }

        public IDecisionesMovimiento Transporte { get; private set; }

        public Term AgrandarAlmacen { get; private set; }
        public Term AlmacenamientoPropio { get; private set; }
        public Term Detencion { get; private set; }
        public Term Ventas { get; set; }
    }


    public class NullDecisionesSemanales : IDecisionesSemanales
    {
        public NullDecisionesSemanales()
        {
            AgrandarAlmacen = 0;
            AlquilarAlmacen = 0;
            UnidadesAProducir = 0;


            Nikelen = new NullDecisionesMovimiento();
            Alternic = new NullDecisionesMovimiento();
            Progesic = new NullDecisionesMovimiento();

            Monopolis = new NullDecisionesPTCiudad();
            Bipolis = new NullDecisionesPTCiudad();
            Tripolis = new NullDecisionesPTCiudad();
            Tetrapolis = new NullDecisionesPTCiudad();
            Metropolis = new NullDecisionesPTCiudad();
        }
        public Term AgrandarAlmacen { get; private set; }
        public Term AlquilarAlmacen { get; private set; }
        public Term UnidadesAProducir { get; set; }

        public IDecisionesMovimiento Nikelen { get; private set; }
        public IDecisionesMovimiento Progesic { get; private set; }
        public IDecisionesMovimiento Alternic { get; private set; }

        public IDecisionesPTCiudad Monopolis { get; private set; }
        public IDecisionesPTCiudad Bipolis { get; private set; }
        public IDecisionesPTCiudad Tripolis { get; private set; }
        public IDecisionesPTCiudad Tetrapolis { get; private set; }
        public IDecisionesPTCiudad Metropolis { get; private set; }
    }

    public class NullDecisionesMovimiento : IDecisionesMovimiento
    {
        public NullDecisionesMovimiento()
        {
            Aereo = 0;
            Terrestre = 0;

            AlmacenPropio = 0;
            AlmacenAlquilado = 0;
            Detencion = 0;
        }
        public Term Aereo { get; private set; }
        public Term Terrestre { get; private set; }
        public Term AlmacenPropio { get; private set; }
        public Term AlmacenAlquilado { get; private set; }
        public Term Detencion { get; private set; }

    }

    public class NullDecisionesPTCiudad : IDecisionesPTCiudad
    {
        public NullDecisionesPTCiudad()
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

    public interface IDecisionesSemanales
    {
        Term AgrandarAlmacen { get; }
        Term AlquilarAlmacen { get; }
        Term UnidadesAProducir { get; set; }

        IDecisionesMovimiento Nikelen { get; }
        IDecisionesMovimiento Progesic { get; }
        IDecisionesMovimiento Alternic { get; }

        IDecisionesPTCiudad Monopolis { get; }
        IDecisionesPTCiudad Bipolis { get; }
        IDecisionesPTCiudad Tripolis { get; }
        IDecisionesPTCiudad Tetrapolis { get; }
        IDecisionesPTCiudad Metropolis { get; }
    }

    public interface IDecisionesMovimiento
    {
        Term Aereo { get; }
        Term Terrestre { get; }
        Term AlmacenPropio { get; }
        Term AlmacenAlquilado { get; }
        Term Detencion { get; }
    }

    public interface IDecisionesPTCiudad
    {
        IDecisionesMovimiento Transporte { get; }
        Term AgrandarAlmacen { get; }
        Term AlmacenamientoPropio { get; }
        Term Detencion { get; }
        Term Ventas { get; set; }
    }

    #endregion

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
                else
                {
                    _nivelDeServicioMínimo = value;
                }
            }
        }

        public enum PrimaPorCarga
        {
            MateriaPrima,
            ProductoTerminado,
            Both
        }

        public ParametrosMateriasPrimas MateriasPrimas { get; set; }

        public ParametrosProductoTerminado ProductoTerminado { get; set; }
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