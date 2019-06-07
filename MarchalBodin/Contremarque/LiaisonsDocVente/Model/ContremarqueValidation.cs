using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Data;

namespace LiaisonsDocVente.Model
{
    /// <summary>
    /// Non utilisé
    /// @see Contremarque
    /// </summary>
    class ContremarqueValidation : ValidationRule
    {
        private ContremarqueRepository _repository = new ContremarqueRepository();

        private readonly Regex _remiseRegex = new Regex("[^0-9+]+");

        public override ValidationResult Validate(object value,
        System.Globalization.CultureInfo cultureInfo)
        {
            Contremarque contremarque = (value as BindingGroup).Items[0] as Contremarque;

            // Check uniquement si coché
            if (!contremarque.RowChecked)
            {
                return ValidationResult.ValidResult;
            }

            // Désignation
            if (contremarque.Design == string.Empty)
            {
                return new ValidationResult(false, "La désignation est obligatoire");
            }

            // Uniquement article DIVERS
            if (contremarque.IsArticleDivers)
            {
                // Fournisseur
                if (contremarque.FournPrinc == string.Empty)
                {
                    return new ValidationResult(false, "Fournisseur principal obligatoire.");
                }

                // Tarif fourn existe?
                if (contremarque.FournPrinc != string.Empty && contremarque.RefFourn != string.Empty)
                {
                    string arRef = _repository.GetArticleFromRefFourn(contremarque.RefFourn, contremarque.FournPrinc);
                    if (arRef != null)
                    {
                        return new ValidationResult(false, $"Référence fournisseur existe déjà sur l'article '{arRef}'");
                    }
                }
            }

            return ValidationResult.ValidResult;
        }
    }
}
