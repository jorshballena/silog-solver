using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.SolverFoundation.Services;
using Newtonsoft.Json;
using SolverFoundation.Plugin.Gurobi;

namespace SilogSolver
{
    public static class Helper
    {
        public static int SemanasDePrimaObligada;
        public static Decision AddVentasSemana(this Model model, bool maximizeUtilidad, int semana, string ciudad, double nivelDeServicioMinimo, uint demanda, Term disponible)
        {
            var ventas = new Decision(Domain.IntegerNonnegative, "s" + semana + "_" + ciudad + "_ventas");
            model.AddDecision(ventas);

            model.AddConstraint("s" + semana + "_" + ciudad + "_ventas_disponibilidad", ventas <= disponible);

            if (maximizeUtilidad && nivelDeServicioMinimo > 0)
            {
                var nds = new Decision(Domain.RealNonnegative, "s" + semana + "_" + ciudad + "_nivelDeServicio");
                model.AddDecision(nds);
                model.AddConstraint("s" + semana + "_const_nivelDeServicio_" + ciudad, nds == ventas / demanda);

                var ventaMinima = Math.Ceiling(demanda * nivelDeServicioMinimo);

                model.AddConstraint("s" + semana + "_ventas_" + ciudad + "_min", ventas >= ventaMinima);
            }

            model.AddConstraint("s" + semana + "_" + ciudad + "_ventas_demanda", ventas <= demanda);

            return ventas;
        }
        public static Term AddRestriccionAlmacenProductoTerminado(this Model model, int semana, Term disponibilidad, Term capacidadAlmacen, IDecisionesCiudad ciudad, string city)
        {
            model.AddConstraint("s" + semana + "_cantidadTotal_PT_" + city, disponibilidad == ciudad.AlmacenamientoPropio + ciudad.Detencion);
            model.AddConstraint("s" + semana + "_capacidadAlmacen_PT_" + city, capacidadAlmacen >= ciudad.AlmacenamientoPropio);
            return SilogParams.CostoFijoAlmacenPropioPT * capacidadAlmacen + SilogParams.CostoVariableAlmacenPropioPT * ciudad.AlmacenamientoPropio + SilogParams.CostoDetencionPT * ciudad.Detencion;
        }
        public static Term AddContainerIncompletoTerrestreMP(this Model model, ParametrosDeEntrada.PrimaPorCarga prima, Term total, int semana, string transp)
        {
            if (semana <= SemanasDePrimaObligada || prima == ParametrosDeEntrada.PrimaPorCarga.Both ||
                prima == ParametrosDeEntrada.PrimaPorCarga.MateriaPrima)
            {
                var container = new Decision(Domain.IntegerNonnegative, "s" + semana + "_dnp_container_terrestre_" + transp);
                var parcial = new Decision(Domain.IntegerNonnegative, "s" + semana + "_dnp_parcial_terrestre_" + transp);

                model.AddDecisions(container, parcial);
                model.AddConstraint("s" + semana + "_containerTotal_terrestre_" + transp,
                    container * SilogParams.CapContenedorTerrestreMP + parcial == total);

                return parcial;
            }
            return 0;
        }
        public static Term AddContainerIncompletoAereoMP(this Model model, ParametrosDeEntrada.PrimaPorCarga prima, Term total, int semana, string transp)
        {
            if (semana <= SemanasDePrimaObligada || prima == ParametrosDeEntrada.PrimaPorCarga.Both ||
                prima == ParametrosDeEntrada.PrimaPorCarga.MateriaPrima)
            {
                var container = new Decision(Domain.IntegerNonnegative, "s" + semana + "_dnp_container_aereo_" + transp);
                var parcial = new Decision(Domain.IntegerNonnegative, "s" + semana + "_dnp_parcial_aereo_" + transp);

                model.AddDecisions(container, parcial);
                model.AddConstraint("s" + semana + "_containerTotal_aereo_" + transp,
                    container * SilogParams.CapContenedorAereoMP + parcial == total);

                return parcial;
            }
            return 0;
        }
        public static Term AddContainerIncompletoTerrestrePT(this Model model, ParametrosDeEntrada.PrimaPorCarga prima, Term total, int semana, string transp)
        {
            if (semana <= SemanasDePrimaObligada || prima == ParametrosDeEntrada.PrimaPorCarga.Both ||
                prima == ParametrosDeEntrada.PrimaPorCarga.ProductoTerminado)
            {
                var container = new Decision(Domain.IntegerNonnegative, "s" + semana + "_dnp_container_terrestre_" + transp);
                var parcial = new Decision(Domain.IntegerNonnegative, "s" + semana + "_dnp_parcial_terrestre_" + transp);

                model.AddDecisions(container, parcial);
                model.AddConstraint("s" + semana + "_containerTotal_terrestre_" + transp,
                    container * SilogParams.CapContenedorTerrestrePT + parcial == total);

                return parcial;
            }
            return 0;
        }
        public static Term AddContainerIncompletoAereoPT(this Model model, ParametrosDeEntrada.PrimaPorCarga prima, Term total, int semana, string transp)
        {
            if (semana <= SemanasDePrimaObligada || prima == ParametrosDeEntrada.PrimaPorCarga.Both || prima == ParametrosDeEntrada.PrimaPorCarga.ProductoTerminado)
            {
                var container = new Decision(Domain.IntegerNonnegative, "s" + semana + "_dnp_container_aereo_" + transp);
                var parcial = new Decision(Domain.IntegerNonnegative, "s" + semana + "_dnp_parcial_aereo_" + transp);

                model.AddDecisions(container, parcial);
                model.AddConstraint("s" + semana + "_containerTotal_aereo_" + transp,
                    container * SilogParams.CapContenedorAereoPT + parcial == total);
                return parcial;
            }
            return 0;
        }
    }

    public class ListDecisionesSemanales : List<IDecisionesSemanales>
    {
        public ListDecisionesSemanales(Model model, int total, string monopolis, string bipolis, string tripolis, string tetrapolis, string metropolis)
        {
            for (int i = 0; i < total; i++)
            {
                Add(new DecisionesSemanales(model, "s" + i, monopolis, bipolis, tripolis, tetrapolis, metropolis));
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
        }
    }

    public static class Program
    {
        static void Main(string[] args)
        {
            SolverContext context = SolverContext.GetContext();
            Model model = context.CreateModel();

            string path = args.Length < 1 ? "params.json" : args[0];

            var data = System.IO.File.ReadAllText(path);
            var parametrosEntrada = JsonConvert.DeserializeObject<ParametrosDeEntrada>(data);

            #region Variables

            var numSemanas = parametrosEntrada.CantidadDeSemanas;

            if (numSemanas < 1)
            {
                Console.WriteLine("No. de Semanas Inválidos");
                Console.ReadLine();
                return;
            }

            if (numSemanas <= 6)
            {
                parametrosEntrada.CalcularPrimaPorCargaIncompleta = ParametrosDeEntrada.PrimaPorCarga.Both;
            }
            else if (numSemanas > 6 &&
                     parametrosEntrada.CalcularPrimaPorCargaIncompleta == ParametrosDeEntrada.PrimaPorCarga.Both)
            {
                Console.WriteLine("No se pueden calcular ambas primas de carga incompleta para más de 6 semenas");
                Console.ReadLine();
                return;
            }

            Helper.SemanasDePrimaObligada = 7 - numSemanas;

            var dSemana = new ListDecisionesSemanales(model, numSemanas,
                parametrosEntrada.ProductoTerminado.Monopolis.CityName,
                parametrosEntrada.ProductoTerminado.Bipolis.CityName,
                parametrosEntrada.ProductoTerminado.Tripolis.CityName,
                parametrosEntrada.ProductoTerminado.Tetrapolis.CityName,
                parametrosEntrada.ProductoTerminado.Metropolis.CityName);
            Term ventasTotal = 0;
            Term demandaTotal = 0;

            #region Produccion
            var consumoAlternic = new Term[numSemanas + 1];
            var disponibleAlternic = new Term[numSemanas + 1];

            var consumoNikelen = new Term[numSemanas + 1];
            var disponibleNikelen = new Term[numSemanas + 1];

            var consumoProgesic = new Term[numSemanas + 1];
            var disponibleProgesic = new Term[numSemanas + 1];

            #endregion

            #region Ventas

            var salidaMonopolis = new Term[numSemanas + 1];
            var disponibleMonopolis = new Term[numSemanas + 1];
            var disponibleBipolis = new Term[numSemanas + 1];
            var disponibleTripolis = new Term[numSemanas + 1];
            var disponibleTetrapolis = new Term[numSemanas + 1];
            var disponibleMetropolis = new Term[numSemanas + 1];

            var ventas = new Term[numSemanas + 1];
            var unidadesVendidas = new Term[numSemanas + 1];
            var demandaTotalEstimada = new Term[numSemanas + 1];

            #endregion

            #region Costos

            var costosProduccion = new Term[numSemanas + 1];

            var capacidadAlmacenPropioMP = new Term[numSemanas + 1];
            var capacidadAlmacenAlquiladoMP = new Term[numSemanas + 1];

            var costoAlmacenamientoMP = new Term[numSemanas + 1];

            var capacidadAlmacenPTMonopolis = new Term[numSemanas + 1];
            var costoAlmacenamientoMonopolis = new Term[numSemanas + 1];
            var capacidadAlmacenPTBipolis = new Term[numSemanas + 1];
            var costoAlmacenamientoBipolis = new Term[numSemanas + 1];
            var capacidadAlmacenPTTripolis = new Term[numSemanas + 1];
            var costoAlmacenamientoTripolis = new Term[numSemanas + 1];
            var capacidadAlmacenPTTetrapolis = new Term[numSemanas + 1];
            var costoAlmacenamientoTetrapolis = new Term[numSemanas + 1];
            var capacidadAlmacenPTMetropolis = new Term[numSemanas + 1];
            var costoAlmacenamientoMetropolis = new Term[numSemanas + 1];
            var costosAlmacenamiento = new Term[numSemanas + 1];

            var costoParcialTerrestreAlternic = new Term[numSemanas + 1];
            var costoParcialAereoAlternic = new Term[numSemanas + 1];
            var costoTransporteAlternic = new Term[numSemanas + 1];
            var costoParcialTerrestreNikelen = new Term[numSemanas + 1];
            var costoParcialAereoNikelen = new Term[numSemanas + 1];
            var costoTransporteNikelen = new Term[numSemanas + 1];
            var costoParcialTerrestreProgesic = new Term[numSemanas + 1];
            var costoParcialAereoProgesic = new Term[numSemanas + 1];
            var costoTransporteProgesic = new Term[numSemanas + 1];

            var costoTransporteMP = new Term[numSemanas + 1];


            var costoParcialTerrestreBipolis = new Term[numSemanas + 1];
            var costoParcialAereoBipolis = new Term[numSemanas + 1];
            var costoTransporteBipolis = new Term[numSemanas + 1];
            var costoParcialTerrestreTripolis = new Term[numSemanas + 1];
            var costoParcialAereoTripolis = new Term[numSemanas + 1];
            var costoTransporteTripolis = new Term[numSemanas + 1];
            var costoParcialTerrestreTetrapolis = new Term[numSemanas + 1];
            var costoParcialAereoTetrapolis = new Term[numSemanas + 1];
            var costoTransporteTetrapolis = new Term[numSemanas + 1];
            var costoParcialTerrestreMetropolis = new Term[numSemanas + 1];
            var costoParcialAereoMetropolis = new Term[numSemanas + 1];
            var costoTransporteMetropolis = new Term[numSemanas + 1];
            var costoTransportePT = new Term[numSemanas + 1];
            var costosTransporte = new Term[numSemanas + 1];

            var gastos = new Term[numSemanas + 1];


            var utilidad = new Term[numSemanas];

            #endregion

            #endregion

            for (int currentWeek = 0; currentWeek <= numSemanas - 1; currentWeek++)
            {
                #region Produccion

                if (currentWeek == 0)
                {
                    disponibleAlternic[currentWeek] = parametrosEntrada.MateriasPrimas.Alternic.Disponible;
                    disponibleNikelen[currentWeek] = parametrosEntrada.MateriasPrimas.Nikelen.Disponible;
                    disponibleProgesic[currentWeek] = parametrosEntrada.MateriasPrimas.Progesic.Disponible;
                }
                else
                {
                    disponibleAlternic[currentWeek] = disponibleAlternic[currentWeek - 1] - consumoAlternic[currentWeek - 1] + dSemana[currentWeek - 1].Alternic.Transporte.Aereo + dSemana[currentWeek - 3].Alternic.Transporte.Terrestre + parametrosEntrada.MateriasPrimas.Alternic.EnTransito[currentWeek - 1];
                    disponibleNikelen[currentWeek] = disponibleNikelen[currentWeek - 1] - consumoNikelen[currentWeek - 1] + dSemana[currentWeek - 1].Nikelen.Transporte.Aereo + dSemana[currentWeek - 2].Nikelen.Transporte.Terrestre + parametrosEntrada.MateriasPrimas.Nikelen.EnTransito[currentWeek - 1];
                    disponibleProgesic[currentWeek] = disponibleProgesic[currentWeek - 1] - consumoProgesic[currentWeek - 1] + dSemana[currentWeek - 2].Progesic.Transporte.Aereo + dSemana[currentWeek - 5].Progesic.Transporte.Terrestre + parametrosEntrada.MateriasPrimas.Progesic.EnTransito[currentWeek - 1];
                }

                consumoAlternic[currentWeek] = SilogParams.AlternicPorProductoTerminado * dSemana[currentWeek].UnidadesAProducir;
                model.AddConstraint("s" + currentWeek + "_consumoMax_alternic", consumoAlternic[currentWeek] <= disponibleAlternic[currentWeek] * SilogParams.ConsumoMaximoDeDisponible);

                consumoNikelen[currentWeek] = SilogParams.NikelenPorProductoTerminado * dSemana[currentWeek].UnidadesAProducir;
                model.AddConstraint("s" + currentWeek + "_consumoMax_nikelen", consumoNikelen[currentWeek] <= disponibleNikelen[currentWeek] * SilogParams.ConsumoMaximoDeDisponible);

                consumoProgesic[currentWeek] = SilogParams.ProgesicPorProductoTerminado * dSemana[currentWeek].UnidadesAProducir;
                model.AddConstraint("s" + currentWeek + "_consumoMax_progesic", consumoProgesic[currentWeek] <= disponibleProgesic[currentWeek] * SilogParams.ConsumoMaximoDeDisponible);

                #endregion

                #region Ventas

                salidaMonopolis[currentWeek] = (dSemana[currentWeek].Bipolis.Transporte.Aereo +
                                                 dSemana[currentWeek].Bipolis.Transporte.Terrestre +
                                                 dSemana[currentWeek].Tripolis.Transporte.Aereo +
                                                 dSemana[currentWeek].Tripolis.Transporte.Terrestre +
                                                 dSemana[currentWeek].Tetrapolis.Transporte.Aereo +
                                                 dSemana[currentWeek].Tetrapolis.Transporte.Terrestre +
                                                 dSemana[currentWeek].Metropolis.Transporte.Aereo +
                                                 dSemana[currentWeek].Metropolis.Transporte.Terrestre);

                if (currentWeek == 0)
                {
                    disponibleMonopolis[0] = parametrosEntrada.ProductoTerminado.Monopolis.Disponible - salidaMonopolis[0];
                    disponibleBipolis[0] = parametrosEntrada.ProductoTerminado.Bipolis.Disponible;
                    disponibleTripolis[0] = parametrosEntrada.ProductoTerminado.Tripolis.Disponible;
                    disponibleTetrapolis[0] = parametrosEntrada.ProductoTerminado.Tetrapolis.Disponible;
                    disponibleMetropolis[0] = parametrosEntrada.ProductoTerminado.Metropolis.Disponible;
                }
                else
                {
                    disponibleMonopolis[currentWeek] = disponibleMonopolis[currentWeek - 1] - dSemana[currentWeek - 1].Monopolis.Ventas + dSemana[currentWeek - 1].UnidadesAProducir - salidaMonopolis[currentWeek];
                    disponibleBipolis[currentWeek] = (disponibleBipolis[currentWeek - 1] - dSemana[currentWeek - 1].Bipolis.Ventas) + dSemana[currentWeek - 1].Bipolis.Transporte.Aereo + dSemana[currentWeek - 3].Bipolis.Transporte.Terrestre + parametrosEntrada.ProductoTerminado.Bipolis.EnTransito[currentWeek - 1];
                    disponibleTripolis[currentWeek] = (disponibleTripolis[currentWeek - 1] - dSemana[currentWeek - 1].Tripolis.Ventas) + dSemana[currentWeek - 1].Tripolis.Transporte.Aereo + dSemana[currentWeek - 3].Tripolis.Transporte.Terrestre + parametrosEntrada.ProductoTerminado.Tripolis.EnTransito[currentWeek - 1];
                    disponibleTetrapolis[currentWeek] = (disponibleTetrapolis[currentWeek - 1] - dSemana[currentWeek - 1].Tetrapolis.Ventas) + dSemana[currentWeek - 1].Tetrapolis.Transporte.Aereo + dSemana[currentWeek - 4].Tetrapolis.Transporte.Terrestre + parametrosEntrada.ProductoTerminado.Tetrapolis.EnTransito[currentWeek - 1];
                    disponibleMetropolis[currentWeek] = (disponibleMetropolis[currentWeek - 1] - dSemana[currentWeek - 1].Metropolis.Ventas) + dSemana[currentWeek - 1].Metropolis.Transporte.Aereo + dSemana[currentWeek - 2].Metropolis.Transporte.Terrestre + parametrosEntrada.ProductoTerminado.Metropolis.EnTransito[currentWeek - 1];
                }

                dSemana[currentWeek].Monopolis.Ventas = model.AddVentasSemana(parametrosEntrada.Maximize == ParametrosDeEntrada.MaximizeParams.Utilidad, currentWeek, parametrosEntrada.ProductoTerminado.Monopolis.CityName, parametrosEntrada.NivelDeServicioMínimo, parametrosEntrada.ProductoTerminado.Monopolis.DemandaEstimada[currentWeek], disponibleMonopolis[currentWeek]);
                dSemana[currentWeek].Bipolis.Ventas = model.AddVentasSemana(parametrosEntrada.Maximize == ParametrosDeEntrada.MaximizeParams.Utilidad, currentWeek, parametrosEntrada.ProductoTerminado.Bipolis.CityName, parametrosEntrada.NivelDeServicioMínimo, parametrosEntrada.ProductoTerminado.Bipolis.DemandaEstimada[currentWeek], disponibleBipolis[currentWeek]);
                dSemana[currentWeek].Tripolis.Ventas = model.AddVentasSemana(parametrosEntrada.Maximize == ParametrosDeEntrada.MaximizeParams.Utilidad, currentWeek, parametrosEntrada.ProductoTerminado.Tripolis.CityName, parametrosEntrada.NivelDeServicioMínimo, parametrosEntrada.ProductoTerminado.Tripolis.DemandaEstimada[currentWeek], disponibleTripolis[currentWeek]);
                dSemana[currentWeek].Tetrapolis.Ventas = model.AddVentasSemana(parametrosEntrada.Maximize == ParametrosDeEntrada.MaximizeParams.Utilidad, currentWeek, parametrosEntrada.ProductoTerminado.Tetrapolis.CityName, parametrosEntrada.NivelDeServicioMínimo, parametrosEntrada.ProductoTerminado.Tetrapolis.DemandaEstimada[currentWeek], disponibleTetrapolis[currentWeek]);
                dSemana[currentWeek].Metropolis.Ventas = model.AddVentasSemana(parametrosEntrada.Maximize == ParametrosDeEntrada.MaximizeParams.Utilidad, currentWeek, parametrosEntrada.ProductoTerminado.Metropolis.CityName, parametrosEntrada.NivelDeServicioMínimo, parametrosEntrada.ProductoTerminado.Metropolis.DemandaEstimada[currentWeek], disponibleMetropolis[currentWeek]);

                unidadesVendidas[currentWeek] = dSemana[currentWeek].Monopolis.Ventas +
                    dSemana[currentWeek].Bipolis.Ventas +
                    dSemana[currentWeek].Tripolis.Ventas +
                    dSemana[currentWeek].Tetrapolis.Ventas +
                    dSemana[currentWeek].Metropolis.Ventas;

                demandaTotalEstimada[currentWeek] =
                    parametrosEntrada.ProductoTerminado.Monopolis.DemandaEstimada[currentWeek] +
                    parametrosEntrada.ProductoTerminado.Bipolis.DemandaEstimada[currentWeek] +
                    parametrosEntrada.ProductoTerminado.Tripolis.DemandaEstimada[currentWeek] +
                    parametrosEntrada.ProductoTerminado.Tetrapolis.DemandaEstimada[currentWeek] +
                    parametrosEntrada.ProductoTerminado.Metropolis.DemandaEstimada[currentWeek];

                var nivelDeServicio = new Decision(Domain.Real, "s" + currentWeek + "_tot_nivelDeServicio");
                model.AddDecision(nivelDeServicio);
                model.AddConstraint("s" + currentWeek + "_constraint_nivelDeServicio", nivelDeServicio == unidadesVendidas[currentWeek] / demandaTotalEstimada[currentWeek]);

                ventasTotal = ventasTotal + unidadesVendidas[currentWeek];
                demandaTotal = demandaTotal + demandaTotalEstimada[currentWeek];

                ventas[currentWeek] = unidadesVendidas[currentWeek] * SilogParams.PrecioDeVenta;

                var decVentas = new Decision(Domain.Real, "s" + currentWeek + "_tot_ventas");
                model.AddDecision(decVentas);
                model.AddConstraint("s" + currentWeek + "const_tot_ventas", decVentas == ventas[currentWeek]);

                #endregion

                #region Gastos

                costosProduccion[currentWeek] = SilogParams.CostoFijoProduccion + SilogParams.CostoVariableProduccion * dSemana[currentWeek].UnidadesAProducir +
                                                 consumoAlternic[currentWeek] * SilogParams.CostoAcarreoAlmacenPropioMP +
                                                 consumoNikelen[currentWeek] * SilogParams.CostoAcarreoAlmacenPropioMP +
                                                 consumoProgesic[currentWeek] * SilogParams.CostoAcarreoAlmacenPropioMP + parametrosEntrada.InversionMantenimiento;

                #region Costos Almacen

                if (currentWeek == 0)
                {
                    capacidadAlmacenPropioMP[currentWeek] = parametrosEntrada.MateriasPrimas.CapacidadActualAlmacen;
                    capacidadAlmacenPTMonopolis[currentWeek] = parametrosEntrada.ProductoTerminado.Monopolis.CapacidadActualAlmacen;
                    capacidadAlmacenPTBipolis[currentWeek] = parametrosEntrada.ProductoTerminado.Bipolis.CapacidadActualAlmacen;
                    capacidadAlmacenPTTripolis[currentWeek] = parametrosEntrada.ProductoTerminado.Tripolis.CapacidadActualAlmacen;
                    capacidadAlmacenPTTetrapolis[currentWeek] = parametrosEntrada.ProductoTerminado.Tetrapolis.CapacidadActualAlmacen;
                    capacidadAlmacenPTMetropolis[currentWeek] = parametrosEntrada.ProductoTerminado.Metropolis.CapacidadActualAlmacen;
                }
                else if (currentWeek == 1)
                {
                    capacidadAlmacenPropioMP[currentWeek] = capacidadAlmacenPropioMP[currentWeek - 1] + parametrosEntrada.MateriasPrimas.CapacidadEnConstruccion;
                    capacidadAlmacenPTMonopolis[currentWeek] = capacidadAlmacenPTMonopolis[currentWeek - 1] + parametrosEntrada.ProductoTerminado.Monopolis.CapacidadEnConstruccion;
                    capacidadAlmacenPTBipolis[currentWeek] = capacidadAlmacenPTBipolis[currentWeek - 1] + parametrosEntrada.ProductoTerminado.Bipolis.CapacidadEnConstruccion;
                    capacidadAlmacenPTTripolis[currentWeek] = capacidadAlmacenPTTripolis[currentWeek - 1] + parametrosEntrada.ProductoTerminado.Tripolis.CapacidadEnConstruccion;
                    capacidadAlmacenPTTetrapolis[currentWeek] = capacidadAlmacenPTTetrapolis[currentWeek - 1] + parametrosEntrada.ProductoTerminado.Tetrapolis.CapacidadEnConstruccion;
                    capacidadAlmacenPTMetropolis[currentWeek] = capacidadAlmacenPTMetropolis[currentWeek - 1] + parametrosEntrada.ProductoTerminado.Metropolis.CapacidadEnConstruccion;
                }
                else
                {
                    capacidadAlmacenPropioMP[currentWeek] = capacidadAlmacenPropioMP[currentWeek - 1] + dSemana[currentWeek - 2].AgrandarAlmacen * SilogParams.CapacidadAdicionalExpansionMP;
                    capacidadAlmacenPTMonopolis[currentWeek] = capacidadAlmacenPTMonopolis[currentWeek - 1] + dSemana[currentWeek - 2].Monopolis.AgrandarAlmacen * SilogParams.CapacidadAdicionalExpansionPT;
                    capacidadAlmacenPTBipolis[currentWeek] = capacidadAlmacenPTBipolis[currentWeek - 1] + dSemana[currentWeek - 2].Bipolis.AgrandarAlmacen * SilogParams.CapacidadAdicionalExpansionPT;
                    capacidadAlmacenPTTripolis[currentWeek] = capacidadAlmacenPTTripolis[currentWeek - 1] + dSemana[currentWeek - 2].Tripolis.AgrandarAlmacen * SilogParams.CapacidadAdicionalExpansionPT;
                    capacidadAlmacenPTTetrapolis[currentWeek] = capacidadAlmacenPTTetrapolis[currentWeek - 1] + dSemana[currentWeek - 2].Tetrapolis.AgrandarAlmacen * SilogParams.CapacidadAdicionalExpansionPT;
                    capacidadAlmacenPTMetropolis[currentWeek] = capacidadAlmacenPTMetropolis[currentWeek - 1] + dSemana[currentWeek - 2].Metropolis.AgrandarAlmacen * SilogParams.CapacidadAdicionalExpansionPT;
                }

                capacidadAlmacenAlquiladoMP[currentWeek] = dSemana[currentWeek - 1].AlquilarAlmacen * SilogParams.CapacidadAlmacenAlquilado;

                model.AddConstraint("s" + currentWeek + "_cantidadAlmancen_alternic", disponibleAlternic[currentWeek] == dSemana[currentWeek].Alternic.Almacen.AlmacenPropio + dSemana[currentWeek].Alternic.Almacen.AlmacenAlquilado + dSemana[currentWeek].Alternic.Almacen.Detencion);
                model.AddConstraint("s" + currentWeek + "_cantidadAlmancen_nikelen", disponibleNikelen[currentWeek] == dSemana[currentWeek].Nikelen.Almacen.AlmacenPropio + dSemana[currentWeek].Nikelen.Almacen.AlmacenAlquilado + dSemana[currentWeek].Nikelen.Almacen.Detencion);
                model.AddConstraint("s" + currentWeek + "_cantidadAlmancen_progesic", disponibleProgesic[currentWeek] == dSemana[currentWeek].Progesic.Almacen.AlmacenPropio + dSemana[currentWeek].Progesic.Almacen.AlmacenAlquilado + dSemana[currentWeek].Progesic.Almacen.Detencion);

                model.AddConstraint("s" + currentWeek + "_capacidadAlmancenPropio_MP", capacidadAlmacenPropioMP[currentWeek] >= dSemana[currentWeek].Alternic.Almacen.AlmacenPropio + dSemana[currentWeek].Nikelen.Almacen.AlmacenPropio + dSemana[currentWeek].Progesic.Almacen.AlmacenPropio);
                model.AddConstraint("s" + currentWeek + "_capacidadAlmancenAlquilado_MP", capacidadAlmacenAlquiladoMP[currentWeek] >= dSemana[currentWeek].Alternic.Almacen.AlmacenAlquilado + dSemana[currentWeek].Nikelen.Almacen.AlmacenAlquilado + dSemana[currentWeek].Progesic.Almacen.AlmacenAlquilado);

                costoAlmacenamientoMP[currentWeek] =
                    dSemana[currentWeek].AgrandarAlmacen * SilogParams.CostoExpansionAlmacenMP +
                    SilogParams.CostoFijoAlmacenPropioMP * capacidadAlmacenPropioMP[currentWeek] +
                    //Costo almacenamiento propio
                    (dSemana[currentWeek].Alternic.Almacen.AlmacenPropio * SilogParams.CostoVariableAlmacenPropioMP) +
                    (dSemana[currentWeek].Nikelen.Almacen.AlmacenPropio * SilogParams.CostoVariableAlmacenPropioMP) +
                    (dSemana[currentWeek].Progesic.Almacen.AlmacenPropio * SilogParams.CostoVariableAlmacenPropioMP) +
                    //Costo almacenamiento alquilado
                    dSemana[currentWeek - 1].AlquilarAlmacen * SilogParams.CostoFijoAlmacenAlquilado +
                    (dSemana[currentWeek].Alternic.Almacen.AlmacenAlquilado * SilogParams.CostoVariableAlmacenAlquilado) +
                    (dSemana[currentWeek].Nikelen.Almacen.AlmacenAlquilado * SilogParams.CostoVariableAlmacenAlquilado) +
                    (dSemana[currentWeek].Progesic.Almacen.AlmacenAlquilado * SilogParams.CostoVariableAlmacenAlquilado) +
                    //Costo en Detencion
                    (dSemana[currentWeek].Alternic.Almacen.Detencion * SilogParams.CostoDetencionMP) +
                    (dSemana[currentWeek].Nikelen.Almacen.Detencion * SilogParams.CostoDetencionMP) +
                    (dSemana[currentWeek].Progesic.Almacen.Detencion * SilogParams.CostoDetencionMP);


                costoAlmacenamientoMonopolis[currentWeek] =
                    dSemana[currentWeek].Monopolis.AgrandarAlmacen * SilogParams.CostoExpansionAlmacenPT +
                    dSemana[currentWeek].Monopolis.Ventas * SilogParams.CostoAcarreoAlmacenPropioPT +
                    model.AddRestriccionAlmacenProductoTerminado(currentWeek, disponibleMonopolis[currentWeek], capacidadAlmacenPTMonopolis[currentWeek], dSemana[currentWeek].Monopolis, parametrosEntrada.ProductoTerminado.Monopolis.CityName);

                costoAlmacenamientoBipolis[currentWeek] =
                    dSemana[currentWeek].Bipolis.AgrandarAlmacen * SilogParams.CostoExpansionAlmacenPT +
                    dSemana[currentWeek].Bipolis.Ventas * SilogParams.CostoAcarreoAlmacenPropioPT +
                    model.AddRestriccionAlmacenProductoTerminado(currentWeek, disponibleBipolis[currentWeek], capacidadAlmacenPTBipolis[currentWeek], dSemana[currentWeek].Bipolis, parametrosEntrada.ProductoTerminado.Bipolis.CityName);

                costoAlmacenamientoTripolis[currentWeek] =
                    dSemana[currentWeek].Tripolis.AgrandarAlmacen * SilogParams.CostoExpansionAlmacenPT +
                    dSemana[currentWeek].Tripolis.Ventas * SilogParams.CostoAcarreoAlmacenPropioPT +
                    model.AddRestriccionAlmacenProductoTerminado(currentWeek, disponibleTripolis[currentWeek], capacidadAlmacenPTTripolis[currentWeek], dSemana[currentWeek].Tripolis, parametrosEntrada.ProductoTerminado.Tripolis.CityName);

                costoAlmacenamientoTetrapolis[currentWeek] =
                    dSemana[currentWeek].Tetrapolis.AgrandarAlmacen * SilogParams.CostoExpansionAlmacenPT +
                    dSemana[currentWeek].Tetrapolis.Ventas * SilogParams.CostoAcarreoAlmacenPropioPT +
                    model.AddRestriccionAlmacenProductoTerminado(currentWeek, disponibleTetrapolis[currentWeek], capacidadAlmacenPTTetrapolis[currentWeek], dSemana[currentWeek].Tetrapolis, parametrosEntrada.ProductoTerminado.Tetrapolis.CityName);

                costoAlmacenamientoMetropolis[currentWeek] =
                    dSemana[currentWeek].Metropolis.AgrandarAlmacen * SilogParams.CostoExpansionAlmacenPT +
                    dSemana[currentWeek].Metropolis.Ventas * SilogParams.CostoAcarreoAlmacenPropioPT +
                    model.AddRestriccionAlmacenProductoTerminado(currentWeek, disponibleMetropolis[currentWeek], capacidadAlmacenPTMetropolis[currentWeek], dSemana[currentWeek].Metropolis, parametrosEntrada.ProductoTerminado.Metropolis.CityName);

                costosAlmacenamiento[currentWeek] =
                    costoAlmacenamientoMP[currentWeek] +
                    costoAlmacenamientoMonopolis[currentWeek] +
                    costoAlmacenamientoBipolis[currentWeek] +
                    costoAlmacenamientoTripolis[currentWeek] +
                    costoAlmacenamientoTetrapolis[currentWeek] +
                    costoAlmacenamientoMetropolis[currentWeek];

                #endregion

                #region Costos Transporte

                #region Materia Prima

                costoParcialTerrestreAlternic[currentWeek] = model.AddContainerIncompletoTerrestreMP(parametrosEntrada.CalcularPrimaPorCargaIncompleta, dSemana[currentWeek].Alternic.Transporte.Terrestre, currentWeek, "alternic");
                costoParcialAereoAlternic[currentWeek] = model.AddContainerIncompletoAereoMP(parametrosEntrada.CalcularPrimaPorCargaIncompleta, dSemana[currentWeek].Alternic.Transporte.Aereo, currentWeek, "alternic");
                costoTransporteAlternic[currentWeek] = (SilogParams.CostoTransporteTierraAlternic * dSemana[currentWeek].Alternic.Transporte.Terrestre + SilogParams.CostoOrdenamientoMP) + ((costoParcialTerrestreAlternic[currentWeek] * SilogParams.CostoTransporteTierraAlternic) * SilogParams.PrimaCargaIncompleta) +
                                                         (SilogParams.CostoTransporteAereoAlternic * dSemana[currentWeek].Alternic.Transporte.Aereo + SilogParams.CostoOrdenamientoMP) + ((costoParcialAereoAlternic[currentWeek] * SilogParams.CostoTransporteAereoAlternic) * SilogParams.PrimaCargaIncompleta);

                costoParcialTerrestreNikelen[currentWeek] = model.AddContainerIncompletoTerrestreMP(parametrosEntrada.CalcularPrimaPorCargaIncompleta, dSemana[currentWeek].Nikelen.Transporte.Terrestre, currentWeek, "nikelen");
                costoParcialAereoNikelen[currentWeek] = model.AddContainerIncompletoAereoMP(parametrosEntrada.CalcularPrimaPorCargaIncompleta, dSemana[currentWeek].Nikelen.Transporte.Aereo, currentWeek, "nikelen");
                costoTransporteNikelen[currentWeek] = (SilogParams.CostoTransporteTierraNikelen * dSemana[currentWeek].Nikelen.Transporte.Terrestre + SilogParams.CostoOrdenamientoMP) + ((costoParcialTerrestreNikelen[currentWeek] * SilogParams.CostoTransporteTierraNikelen) * SilogParams.PrimaCargaIncompleta) +
                                                        (SilogParams.CostoTransporteAereoNikelen * dSemana[currentWeek].Nikelen.Transporte.Aereo + SilogParams.CostoOrdenamientoMP) + ((costoParcialAereoNikelen[currentWeek] * SilogParams.CostoTransporteAereoNikelen) * SilogParams.PrimaCargaIncompleta);

                costoParcialTerrestreProgesic[currentWeek] = model.AddContainerIncompletoTerrestreMP(parametrosEntrada.CalcularPrimaPorCargaIncompleta, dSemana[currentWeek].Progesic.Transporte.Terrestre, currentWeek, "progesic");
                costoParcialAereoProgesic[currentWeek] = model.AddContainerIncompletoAereoMP(parametrosEntrada.CalcularPrimaPorCargaIncompleta, dSemana[currentWeek].Progesic.Transporte.Aereo, currentWeek, "progesic");
                costoTransporteProgesic[currentWeek] = (SilogParams.CostoTransporteTierraProgesic * dSemana[currentWeek].Progesic.Transporte.Terrestre + SilogParams.CostoOrdenamientoMP) + ((costoParcialTerrestreProgesic[currentWeek] * SilogParams.CostoTransporteTierraProgesic) * SilogParams.PrimaCargaIncompleta) +
                                                         (SilogParams.CostoTransporteAereoProgesic * dSemana[currentWeek].Progesic.Transporte.Aereo + SilogParams.CostoOrdenamientoMP) + ((costoParcialAereoProgesic[currentWeek] * SilogParams.CostoTransporteAereoProgesic) * SilogParams.PrimaCargaIncompleta);

                costoTransporteMP[currentWeek] = costoTransporteAlternic[currentWeek] +
                                                   costoTransporteNikelen[currentWeek] +
                                                   costoTransporteProgesic[currentWeek];

                #endregion

                #region Producto Terminado

                costoParcialTerrestreBipolis[currentWeek] = model.AddContainerIncompletoTerrestrePT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, dSemana[currentWeek].Bipolis.Transporte.Terrestre, currentWeek, "bipolis");
                costoParcialAereoBipolis[currentWeek] = model.AddContainerIncompletoAereoPT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, dSemana[currentWeek].Bipolis.Transporte.Aereo, currentWeek, "bipolis");
                costoTransporteBipolis[currentWeek] = (SilogParams.CostoTransporteTierra1000 * dSemana[currentWeek].Bipolis.Transporte.Terrestre + SilogParams.CostoOrdenamientoMP) + ((costoParcialTerrestreBipolis[currentWeek] * SilogParams.CostoTransporteTierra1000) * SilogParams.PrimaCargaIncompleta) +
                                                        (SilogParams.CostoTransporteAereo1000 * dSemana[currentWeek].Bipolis.Transporte.Aereo + SilogParams.CostoOrdenamientoMP) + ((costoParcialAereoBipolis[currentWeek] * SilogParams.CostoTransporteAereo1000) * SilogParams.PrimaCargaIncompleta);

                costoParcialTerrestreTripolis[currentWeek] = model.AddContainerIncompletoTerrestrePT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, dSemana[currentWeek].Tripolis.Transporte.Terrestre, currentWeek, "tripolis");
                costoParcialAereoTripolis[currentWeek] = model.AddContainerIncompletoAereoPT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, dSemana[currentWeek].Tripolis.Transporte.Aereo, currentWeek, "tripolis");
                costoTransporteTripolis[currentWeek] = (SilogParams.CostoTransporteTierra1000 * dSemana[currentWeek].Tripolis.Transporte.Terrestre + SilogParams.CostoOrdenamientoMP) + ((costoParcialTerrestreTripolis[currentWeek] * SilogParams.CostoTransporteTierra1000) * SilogParams.PrimaCargaIncompleta) +
                                                         (SilogParams.CostoTransporteAereo1000 * dSemana[currentWeek].Tripolis.Transporte.Aereo + SilogParams.CostoOrdenamientoMP) + ((costoParcialAereoTripolis[currentWeek] * SilogParams.CostoTransporteAereo1000) * SilogParams.PrimaCargaIncompleta);


                costoParcialTerrestreTetrapolis[currentWeek] = model.AddContainerIncompletoTerrestrePT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, dSemana[currentWeek].Tetrapolis.Transporte.Terrestre, currentWeek, "tetrapolis");
                costoParcialAereoTetrapolis[currentWeek] = model.AddContainerIncompletoAereoPT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, dSemana[currentWeek].Tetrapolis.Transporte.Aereo, currentWeek, "tetrapolis");
                costoTransporteTetrapolis[currentWeek] = (SilogParams.CostoTransporteTierra1400 * dSemana[currentWeek].Tetrapolis.Transporte.Terrestre + SilogParams.CostoOrdenamientoMP) + ((costoParcialTerrestreTetrapolis[currentWeek] * SilogParams.CostoTransporteTierra1400) * SilogParams.PrimaCargaIncompleta) +
                                                           (SilogParams.CostoTransporteAereo1400 * dSemana[currentWeek].Tetrapolis.Transporte.Aereo + SilogParams.CostoOrdenamientoMP) + ((costoParcialAereoTetrapolis[currentWeek] * SilogParams.CostoTransporteAereo1400) * SilogParams.PrimaCargaIncompleta);


                costoParcialTerrestreMetropolis[currentWeek] = model.AddContainerIncompletoTerrestrePT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, dSemana[currentWeek].Metropolis.Transporte.Terrestre, currentWeek, "metropolis");
                costoParcialAereoMetropolis[currentWeek] = model.AddContainerIncompletoAereoPT(parametrosEntrada.CalcularPrimaPorCargaIncompleta, dSemana[currentWeek].Metropolis.Transporte.Aereo, currentWeek, "metropolis");
                costoTransporteMetropolis[currentWeek] = (SilogParams.CostoTransporteTierra700 * dSemana[currentWeek].Metropolis.Transporte.Terrestre + SilogParams.CostoOrdenamientoMP) + ((costoParcialTerrestreMetropolis[currentWeek] * SilogParams.CostoTransporteTierra700) * SilogParams.PrimaCargaIncompleta) +
                                                           (SilogParams.CostoTransporteAereo700 * dSemana[currentWeek].Metropolis.Transporte.Aereo + SilogParams.CostoOrdenamientoMP) + ((costoParcialAereoMetropolis[currentWeek] * SilogParams.CostoTransporteAereo700) * SilogParams.PrimaCargaIncompleta);

                costoTransportePT[currentWeek] = costoTransporteBipolis[currentWeek] +
                                                   costoTransporteTripolis[currentWeek] +
                                                   costoTransporteTetrapolis[currentWeek] +
                                                   costoTransporteMetropolis[currentWeek];

                #endregion

                costosTransporte[currentWeek] = costoTransporteMP[currentWeek] + costoTransportePT[currentWeek];

                #endregion


                var decProduccion = new Decision(Domain.Real, "s" + currentWeek + "_tot_gastos_produccion");
                model.AddDecision(decProduccion);
                model.AddConstraint("s" + currentWeek + "const_tot_gastos_produccion", decProduccion == costosProduccion[currentWeek]);

                var decTransporte = new Decision(Domain.Real, "s" + currentWeek + "_tot_gastos_transporte");
                model.AddDecision(decTransporte);
                model.AddConstraint("s" + currentWeek + "const_tot_gastos_transporte", decTransporte == costosTransporte[currentWeek]);

                var decAlmacenamiento = new Decision(Domain.Real, "s" + currentWeek + "_tot_gastos_almacenamiento");
                model.AddDecision(decAlmacenamiento);
                model.AddConstraint("s" + currentWeek + "const_tot_gastos_almacenamiento", decAlmacenamiento == costosAlmacenamiento[currentWeek]);

                gastos[currentWeek] = costosProduccion[currentWeek] + costosTransporte[currentWeek] + costosAlmacenamiento[currentWeek];

                #endregion

                utilidad[currentWeek] = ventas[currentWeek] - gastos[currentWeek];
            }

            #region semana N+1

            var dispFinalAlternic = new Decision(Domain.IntegerNonnegative, "totales_disponibilidad_Alternic_Final");
            disponibleAlternic[numSemanas] = disponibleAlternic[numSemanas - 1] - consumoAlternic[numSemanas - 1] + dSemana[numSemanas - 1].Alternic.Transporte.Aereo + dSemana[numSemanas - 3].Alternic.Transporte.Terrestre;
            model.AddDecision(dispFinalAlternic);
            model.AddConstraint("decisionPT_Alternic", dispFinalAlternic == disponibleAlternic[numSemanas]);
            model.AddConstraint("s" + numSemanas + "_consumoMax_alternic", disponibleAlternic[numSemanas] >= 25000);

            var dispFinalNikelen = new Decision(Domain.IntegerNonnegative, "totales_disponibilidad_Nikelen_Final");
            disponibleNikelen[numSemanas] = disponibleNikelen[numSemanas - 1] - consumoNikelen[numSemanas - 1] + dSemana[numSemanas - 1].Nikelen.Transporte.Aereo + dSemana[numSemanas - 2].Nikelen.Transporte.Terrestre;
            model.AddDecision(dispFinalNikelen);
            model.AddConstraint("decisionPT_Nikelen", dispFinalNikelen == disponibleNikelen[numSemanas]);
            model.AddConstraint("s" + numSemanas + "_consumoMax_nikelen", disponibleNikelen[numSemanas] >= 17000);

            var dispFinalProgesic = new Decision(Domain.IntegerNonnegative, "totales_disponibilidad_Progesic_Final");
            disponibleProgesic[numSemanas] = disponibleProgesic[numSemanas - 1] - consumoProgesic[numSemanas - 1] + dSemana[numSemanas - 2].Progesic.Transporte.Aereo + dSemana[numSemanas - 5].Progesic.Transporte.Terrestre;
            model.AddDecision(dispFinalProgesic);
            model.AddConstraint("decisionPT_Progesic", dispFinalProgesic == disponibleProgesic[numSemanas]);
            model.AddConstraint("s" + numSemanas + "_consumoMax_progesic", disponibleProgesic[numSemanas] >= 9000);

            Term disponibilidadFinal =
                //Disponible Monopolis
                disponibleMonopolis[numSemanas - 1] - dSemana[numSemanas - 1].Monopolis.Ventas + dSemana[numSemanas - 1].UnidadesAProducir +

                //Disponible Bipolis
                (disponibleBipolis[numSemanas - 1] - dSemana[numSemanas - 1].Bipolis.Ventas) + dSemana[numSemanas - 1].Bipolis.Transporte.Aereo + dSemana[numSemanas - 3].Bipolis.Transporte.Terrestre +
                //En transitoBipolis
                dSemana[numSemanas - 2].Bipolis.Transporte.Terrestre + dSemana[numSemanas - 1].Bipolis.Transporte.Terrestre +

                //Disponible Tripolis
                (disponibleTripolis[numSemanas - 1] - dSemana[numSemanas - 1].Tripolis.Ventas) + dSemana[numSemanas - 1].Tripolis.Transporte.Aereo + dSemana[numSemanas - 3].Tripolis.Transporte.Terrestre +
                //En transitoTripolis
                dSemana[numSemanas - 2].Tripolis.Transporte.Terrestre + dSemana[numSemanas - 1].Tripolis.Transporte.Terrestre +

                //Disponible Tetrapolis
                (disponibleTetrapolis[numSemanas - 1] - dSemana[numSemanas - 1].Tetrapolis.Ventas) + dSemana[numSemanas - 1].Tetrapolis.Transporte.Aereo + dSemana[numSemanas - 4].Tetrapolis.Transporte.Terrestre +
                //En transitoTetrapolis
                dSemana[numSemanas - 3].Tetrapolis.Transporte.Terrestre + dSemana[numSemanas - 2].Tetrapolis.Transporte.Terrestre + dSemana[numSemanas - 1].Tetrapolis.Transporte.Terrestre +

                //Disponible Metropolis
                (disponibleMetropolis[numSemanas - 1] - dSemana[numSemanas - 1].Metropolis.Ventas) + dSemana[numSemanas - 1].Metropolis.Transporte.Aereo + dSemana[numSemanas - 2].Metropolis.Transporte.Terrestre +
                //En transitoMetropolis
                dSemana[numSemanas - 1].Metropolis.Transporte.Terrestre;

            var dispFinal = new Decision(Domain.IntegerNonnegative, "totales_disponibilidad_PT_Final");
            model.AddDecision(dispFinal);
            model.AddConstraint("decisionPT_Final", dispFinal == disponibilidadFinal);
            model.AddConstraint("disponiblePT_Final", disponibilidadFinal >= 5000);

            #endregion

            #region NivelDeServicio

            ventasTotal = parametrosEntrada.VentasPrevias + ventasTotal;
            demandaTotal = parametrosEntrada.DemandasPrevias + demandaTotal;

            var nivelDeServicioAcumulado = new Decision(Domain.Real, "totales_nivelDeServicio");
            model.AddDecision(nivelDeServicioAcumulado);
            model.AddConstraint("totales_const_nivelDeServicio", nivelDeServicioAcumulado == ventasTotal / demandaTotal);

            for (var i = 0; i < utilidad.Length; i++)
            {
                var dec = new Decision(Domain.Real, "s" + i + "_tot_Utilidad");
                model.AddDecision(dec);
                model.AddConstraint("s" + i + "_const_utilidad", dec == utilidad[i]);
            }

            #endregion

            #region Utilidad

            var utilidadTerm = parametrosEntrada.UtilidadAcumulada + utilidad.Aggregate((Term)0, (term, term1) => term + term1);

            var decUtilidad = new Decision(Domain.Real, "totales_Utilidad");
            model.AddDecision(decUtilidad);

            model.AddConstraint("totales_const_Utilidad", decUtilidad == utilidadTerm);

            #endregion

            #region Goals

            if (parametrosEntrada.Maximize == ParametrosDeEntrada.MaximizeParams.Utilidad)
            {
                model.AddGoal("maximizarUtilidad", GoalKind.Maximize, utilidadTerm);
            }
            else if (parametrosEntrada.Maximize == ParametrosDeEntrada.MaximizeParams.NivelDeServicio)
            {
                model.AddConstraint("totales_const_Utilidad_Min", decUtilidad >= parametrosEntrada.UtilidadMínima);
                model.AddGoal("maximizarNivelDeServicio", GoalKind.Maximize, nivelDeServicioAcumulado);
            }
            else
            {
                var uP = new Decision(Domain.Real, "totales_dnp_score_utilidad");
                var nP = new Decision(Domain.Real, "totales_dnp_score_nivelDeServicio");

                var utilPonderado = (decUtilidad - parametrosEntrada.ScoresEstimados.UtilidadMinima) / (parametrosEntrada.ScoresEstimados.UtilidadMaxima - parametrosEntrada.ScoresEstimados.UtilidadMinima) * 50;
                var ndsPonderado = (nivelDeServicioAcumulado - parametrosEntrada.ScoresEstimados.NivelDeServicioMinimo) / (parametrosEntrada.ScoresEstimados.NivelDeServicioMaximo - parametrosEntrada.ScoresEstimados.NivelDeServicioMinimo) * 35;

                model.AddDecisions(uP, nP);
                model.AddConstraint("totales_score_const_utlidad", uP == utilPonderado);
                model.AddConstraint("totales_score_const_nds", nP == ndsPonderado);


                model.AddGoal("maximizarAcumulado", GoalKind.Maximize, utilPonderado + ndsPonderado);

            }

            #endregion

            #region PrintResults

            Console.WriteLine(path);
            Console.WriteLine("====== Modelo Construído ======\n");

            try
            {
                Solution solution = context.Solve(new GurobiDirective());
                Console.WriteLine("====== Modelo Resuelto ======\n");
                Report report = solution.GetReport(ReportVerbosity.SolverDetails);

                var ordered = solution.Decisions.GroupBy(c => c.Name.Substring(0, c.Name.IndexOf('_')));

                var result = new StringBuilder();

                result.Append(string.Format("{0}", report));

                foreach (var t in ordered)
                {
                    result.AppendLine();
                    result.AppendLine();
                    result.AppendLine((" ".PadLeft(20, '=') + t.Key + " ").PadRight(50, '='));

                    IGrouping<string, Decision> t1 = t;
                    var subG = t.GroupBy(c => GetNextIdentifier(c.Name, t1.Key.Length + 1));

                    foreach (var subGg in subG)
                    {
                        if (subGg.Key == "dnp")
                            continue;

                        result.AppendLine((" ".PadLeft(20, '-') + subGg.Key + " ").PadRight(50, '-'));

                        foreach (var dec in subGg.OrderBy(c => c.Name))
                        {
                            result.AppendLine(string.Format("{0}: {1}", dec.Name, Math.Round(dec.ToDouble(), 3).ToString("##,0.####")));
                        }
                        result.AppendLine();
                    }
                }

                Console.Write(result.ToString());

                Console.WriteLine();

                Console.WriteLine("Guardar resultados en disco? Y/N");

                var rr = Console.ReadLine();

                if (rr.Trim().ToLowerInvariant() == "y")
                {
                    var outPath = path.Replace(".json", "") + ".result.txt";

                    System.IO.File.WriteAllText(outPath, result.ToString());
                }


            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.InnerException.Message);
                Console.ReadLine();
            }
            #endregion
        }

        private static string GetNextIdentifier(string id, int prefixLength)
        {
            var f = id.Substring(prefixLength);

            if (!f.Contains("_"))
                return f;

            return f.Substring(0, f.IndexOf('_'));
        }

    }
}