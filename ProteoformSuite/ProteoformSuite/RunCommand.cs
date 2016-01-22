using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ManyConsole;

namespace ProteoformSuite
{
    class RunCommand : ConsoleCommand
    {
        public RunCommand()
        {
            this.IsCommand("Run", "Execute full ProteoformSuite pipeline");

            // TODO: implement arguments using HasOption method from ManyConsole namespace
            // TODO: handle null input with ?? null-coalescing operator
            HasOption("f|proteoform_xml=", "Enter the path of a preconstructed proteoform database.", s => ProteoformDatabase = s);
            HasOption("u|uniprot_xml=", "Enter the path of a uniprot database.", s => UniProtXml = s);
            HasOption("e|ensembl_faa=", "Enter the path of an ensembl protein fasta database.", s => EnsemblFasta = s);
        }

        public override int Run(string[] remainingArguments)
        {
            // Database Processing
            // TODO: Allow user to use database from preinstalled resources
            DatabaseGenerator database_generator = new DatabaseGenerator();
            database_generator.Generate(proteoformDb, UniProtDb, EnsemblDb, other_params);

            // Deconvolution
            Deconvolver deconvolver = new Deconvolver(rawFile input, <other_params>)
            deconvolver.RunDeconvolution()

            // Peak Processing
            PeakProcessor peak_processor = new PeakProcessor(deconvolver, other_parameters);
            peak_processor.ProcessPeaks();

            // Pair Proteoforms
            ProteoformMatcher matcher = new ProteoformMatcher(peak_processor, database_generator, other_params);
            matcher.MatchProteoforms(some_params);
            

            // Form Proteoform Families
            FamilyFormer former = new FamilyFormer(matcher, some_params);
            former.FormFamilies();
            // TODO: Output visualization file

            // TODO: Post-Processing:
            ResultsGenerator results = ResultsGenerator(some_parameters);
            results.Generate();
            results.Export();
            // TODO: Export proteoformXML to repository 
            // TODO: Export proteoformXML to Top-Down Proteomics Consortium (TDP)?

            return 0;
        }
    }
}
