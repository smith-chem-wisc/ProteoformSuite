using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManyConsole;


namespace ProteoformSuite
{
    class ProteoformDatabaseCommand : ConsoleCommand
    {
        public ProteoformDatabaseCommand()
        {
            this.IsCommand("Proteoform_Database", "Builds the proteoform database only.");

            // TODO: implement arguments using HasOption method from ManyConsole namespace
            // TODO: handle null input with ?? null-coalescing operator
            HasOption("f|proteoform_xml=", "Enter the path of a preconstructed proteoform database.", s => ProteoformDatabase = s);
            HasOption("u|uniprot_xml=", "Enter the path of a uniprot database.", s => UniProtXml = s);
            HasOption("e|ensembl_faa=", "Enter the path of an ensembl protein fasta database.", s => EnsemblFasta = s);

        }

        public string ProteoformDatabase;
        public string UniProtXml;
        public string EnsemblFasta;

        public override int Run(string[] remainingArguments)
        {
            // TODO: Database Processing:
            // TODO: Allow user to use database from preinstalled resources
            DatabaseGenerator database_generator = new DatabaseGenerator();
            database_generator.Generate(proteoformDb, UniProtDb, EnsemblDb, other_params);
            database_generator.Export();

            return 0;
        }
    }
}
