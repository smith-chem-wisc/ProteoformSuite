# ProteoformSuite
[![Build status](https://ci.appveyor.com/api/projects/status/qbc3xy4b35otnsxe/branch/master?svg=true)](https://ci.appveyor.com/project/stefanks/proteoform-suite/branch/master)
[![Coverage Status](https://coveralls.io/repos/github/smith-chem-wisc/proteoform-suite/badge.svg?branch=master)](https://coveralls.io/github/smith-chem-wisc/proteoform-suite?branch=master)

## License

The software is currently released under the [GNU GPLv3](http://www.gnu.org/licenses/gpl.txt).

Copyright 2016
# ProteoformSuite
A integrated proteoform identification, quantification and GO analyis software.

## Functionality
* Proteoform Identification: Proteoform are identified from measurements of intact mass and lysine count.
* Custom Database: PTMs archived and UniProt and discovered using [G-PTM-D](https://github.com/smith-chem-wisc/gptmd) or [MetaMopheus](https://github.com/smith-chem-wisc/MetaMorpheus) are used in the construction of theoretical proteoforms.
* Quantification: NeuCode labeled proteoform intensity ratios are calculated from separate analysis files following proteoform identification.
* Gene Ontology Analysis: occurs automatically based on experimental proteoforms that are significantly induced or repressed. GO terms are obtained from the UniProt XML database of theoretical proteins.

### Requirements

The following files must be obtained and installed:

* uniprot.xml: A UniProt reference database in .xml format

  [http://www.uniprot.org](http://www.uniprot.org)

* ptmlist.txt: A PTM library
 
  [http://www.uniprot.org/docs/ptmlist.txt](http://www.uniprot.org/docs/ptmlist.txt) 

* for thermo .RAW files: [Thermo MSFileReader](https://thermo.flexnetoperations.com/control/thmo/search?query=MSFileReader)

### System Requirements and Usage for Release Version
- X GB of RAM is recommended
- For thermo .RAW files: Need to have [Thermo MSFileReader 3.1 SP2](https://thermo.flexnetoperations.com/control/thmo/search?query=MSFileReader) installed.

### System Requirements for Editing Solution
- Visual Studio 2017 Version 15.1
- Microsoft .NET Framework Version 4.6
- For thermo .RAW files: Need to have [Thermo MSFileReader 3.1 SP2](https://thermo.flexnetoperations.com/control/thmo/search?query=MSFileReader) installed.

### References

* [Elucidating Proteoform Families from Proteoform Intact-Mass and Lysine-Count Measurements--J. Proteome Res., 2016, 15 (4), pp 1213–1221](http://pubs.acs.org/doi/abs/10.1021/acs.jproteome.5b01090)

* [Global Post-translational Modification Discovery--J. Proteome Res., 2016 Just Accepted](http://pubs.acs.org/doi/abs/10.1021/acs.jproteome.6b00034)

* [Global Identification of Protein Post-translational Modifications in a Single-Pass Database Search--J. Proteome Res., 2015, 14 (11), pp 4714–4720](http://pubs.acs.org/doi/abs/10.1021/acs.jproteome.5b00599)

* [A Proteomics Search Algorithm Specifically Designed for High-Resolution Tandem Mass Spectra--J. Proteome Res., 2013, 12 (3), pp 1377–1386](http://pubs.acs.org/doi/abs/10.1021/pr301024c)
