using MBCore.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ListeCommandes.Model
{
    public class CommandeFiltre
    {
        public List<CommandeType> CommandeTypes { get; set; }
        public List<ApcType> ApcTypes { get; set; }
        public List<CommandeStatut> CommandeStatuts { get; set; }
        public int DateLivPeriode { get; set; }
        public DateTime? DateLivFrom { get; set; }
        public DateTime? DateLivTo { get; set; }
        public DateTime? DateDocFrom { get; set; }
        public Collaborateur Collabo { get; set; }
        public override string ToString()
        {
            string sql = string.Empty;
            StringBuilder builder = new StringBuilder();

            foreach (CommandeType commandeType in CommandeTypes)
            {
                builder.Append((int)commandeType).Append(',');
            }

            sql += " AND DE.DO_Type IN(" + builder.ToString().TrimEnd(',') + ")";

            builder = new StringBuilder();
            foreach (CommandeStatut statut in CommandeStatuts)
            {
                builder.Append((int)statut).Append(',');
            }

            sql += " AND DE.DO_Statut IN(" + builder.ToString().TrimEnd(',') + ")";

            string dateLivPeriod = string.Empty;
            if (DateLivPeriode > 0)
            {
                dateLivPeriod = $" AND YEAR(DE.DO_DateLivr) = YEAR(current_timestamp)";
                // Mois en cours
                if (DateLivPeriode == 1)
                {
                    dateLivPeriod += $" AND MONTH(DE.DO_DateLivr) = MONTH(current_timestamp)";
                }
            }
            sql += dateLivPeriod;

            if (DateLivFrom != null)
            {
                sql += $" AND DE.DO_DateLivr >= @DateLivFrom";
            }

            if (DateLivTo != null)
            {
                sql += $" AND DE.DO_DateLivr <= @DateLivTo";
            }

            if (DateDocFrom != null)
            {
                sql += $" AND DE.DO_Date >= @DateDocFrom";
            }

            sql += (Collabo == null || Collabo.Id == 0) ? string.Empty :$" AND DE.CO_No = {Collabo.Id}";

            if (ApcTypes.Count > 0 && ApcTypes.Count < 3)
            {
                builder = new StringBuilder();
                foreach (ApcType item in ApcTypes)
                {
                    switch (item)
                    {
                        case ApcType.DemandeDePrixClient:
                            builder.Append("'DMD PRIX CLIENT',");
                            break;
                        case ApcType.DemandeDePrixStock:
                            builder.Append("'DMD PRIX STOCK',");
                            break;
                        case ApcType.Reservation:
                            builder.Append("'RESERVATION',");
                            break;
                        case ApcType.Stock:
                            builder.Append("'STOCK',");
                            break;
                        default:
                            break;
                    }
                }

                sql += " AND DE.DO_Ref IN(" + builder.ToString().TrimEnd(',') + ")";
            }
            
            return sql;
        }
    }
}
