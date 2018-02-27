using Accord.Math;
using Proteomics;
using System.Linq;

namespace ProteoformSuiteInternal
{
    public enum Aspect
    {
        MolecularFunction,
        CellularComponent,
        BiologicalProcess
    }

    public class GoTerm
    {
        #region Public Constructors

        public GoTerm(DatabaseReference goTerm)
        {
            Id = goTerm.Id.Split(':')[1].ToString();
            string full_description = goTerm.Properties.Where(prop => prop.Item1 == "term").First().Item2;
            this.Description = full_description.Split(':')[1].ToString();
            switch (full_description.Split(':')[0].ToString())
            {
                case "C":
                    this.Aspect = Aspect.CellularComponent;
                    break;

                case "F":
                    this.Aspect = Aspect.MolecularFunction;
                    break;

                case "P":
                    this.Aspect = Aspect.BiologicalProcess;
                    break;
            }
        }

        public GoTerm(string id, string descritpion, Aspect aspect)
        {
            this.Id = id;
            this.Description = descritpion;
            this.Aspect = aspect;
        }

        #endregion Public Constructors

        #region Public Properties

        public string Id { get; private set; }
        public string Description { get; private set; }
        public Aspect Aspect { get; private set; }

        #endregion Public Properties

        public override string ToString()
        {
            return "GO: " + Id + "; " + Aspect.ToString() + ": " + Description;
        }
    }
}