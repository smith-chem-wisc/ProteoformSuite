# Proteoform Suite
Software for constructing, quantifying, and visualizing proteoform families.
[![Build status](https://ci.appveyor.com/api/projects/status/0r09noqpa7j3687h/branch/master?svg=true)](https://ci.appveyor.com/project/smith-chem-wisc/proteoformsuite/branch/master)
[![codecov](https://codecov.io/gh/smith-chem-wisc/ProteoformSuite/branch/master/graph/badge.svg)](https://codecov.io/gh/smith-chem-wisc/ProteoformSuite)
[![Codacy Badge](https://api.codacy.com/project/badge/Grade/932cdbf7f3694271bb03abb5dbba036c)](https://www.codacy.com/app/acesnik/ProteoformSuite?utm_source=github.com&amp;utm_medium=referral&amp;utm_content=smith-chem-wisc/ProteoformSuite&amp;utm_campaign=Badge_Grade)

## Papers and Software Releases

Proteoform Suite: Software for Constructing, Quantifying, and Visualizing Proteoform Families. [(2017, _J. Proteome Res._)](http://pubs.acs.org/doi/10.1021/acs.jproteome.7b00685)

* The release and vignette for this analysis can be found [here](https://github.com/smith-chem-wisc/ProteoformSuite/releases/tag/0.2.8).

Video tutorial on how to use Proteoform Suite:
[![Proteoform Suite Tutorial](https://img.youtube.com/vi/P17Y_aNYbAM/0.jpg)](https://youtu.be/P17Y_aNYbAM)

Expanding Proteoform Identifications in Top-Down Proteomic Analyses by Constructing Proteoform Families [(2017, _Anal. Chem._)](http://pubs.acs.org/doi/abs/10.1021/acs.analchem.7b04221)
* The release and vignette for top-down analysis can be found [here](https://github.com/smith-chem-wisc/ProteoformSuite/releases/tag/0.3.0).

Elucidating _Escherichia coli_ Proteoform Families Using Intact-Mass Proteomics and a Global PTM Discovery Database. [(2017, _J. Proteome Res._)](http://pubs.acs.org/doi/abs/10.1021/acs.jproteome.7b00516)
* The version used for this analysis can be found [here](https://github.com/smith-chem-wisc/ProteoformSuite/releases/tag/0.1.12).

Vignettes are housed in releases and also at [this repository](https://github.com/smith-chem-wisc/ProteoformSuiteVignettes).

## Functionality
* Proteoform Identification: Proteoform are identified from measurements of intact mass and lysine count.
* Custom Database: PTMs archived and UniProt and discovered using [G-PTM-D](https://github.com/smith-chem-wisc/gptmd) or [MetaMopheus](https://github.com/smith-chem-wisc/MetaMorpheus) are used in the construction of theoretical proteoforms.
* Quantification: NeuCode-labeled proteoform intensity ratios are calculated from separate analysis files following proteoform identification.
* Gene Ontology Analysis: Performed alongside quantification based on proteoforms that are significantly induced or repressed. GO terms are obtained from UniProt protein annotations.
* Visualization: ProteoformSuite facilitates the visualization of the relationships between proteoforms by creating scripts to load in the network visualization program Cytoscape. 

### Requirements

The following files must be obtained and installed:

* uniprot.xml: A UniProt reference database in .xml format

  [http://www.uniprot.org](http://www.uniprot.org)

* ptmlist.txt: A PTM library
 
  [http://www.uniprot.org/docs/ptmlist.txt](http://www.uniprot.org/docs/ptmlist.txt) 

* for thermo .RAW files: [Thermo MSFileReader](https://thermo.flexnetoperations.com/control/thmo/search?query=MSFileReader)

### System Requirements and Usage for Release Version
- 8 GB of RAM is recommended for yeast; more RAM is required for larger databases.
- For visualization of proteoform families: Need to have [Cytoscape 3.5.0](http://cytoscape.org/).
- For visualization of quantitative proteoform families: Need to install enhancedGraphics in Cytoscape using the App Manager under the Tools menu. 
- For thermo .RAW files: Need to have [Thermo MSFileReader 3.1 SP2](https://thermo.flexnetoperations.com/control/thmo/search?query=MSFileReader) installed.

### System Requirements for Editing Solution
- Visual Studio 2017 Version 15.1
- Microsoft .NET Framework Version 4.6
- 16 GB of RAM is recommended
- For visualization of quantitative proteoform families: Need to install enhancedGraphics in Cytoscape using the App Manager under the Tools menu. 
- For thermo .RAW files: Need to have [Thermo MSFileReader 3.1 SP2](https://thermo.flexnetoperations.com/control/thmo/search?query=MSFileReader) installed.

### References

* [Elucidating Proteoform Families from Proteoform Intact-Mass and Lysine-Count Measurements--J. Proteome Res., 2016, 15 (4), pp 1213–1221](http://pubs.acs.org/doi/abs/10.1021/acs.jproteome.5b01090)

* [Global Post-translational Modification Discovery--J. Proteome Res., 2016 Just Accepted](http://pubs.acs.org/doi/abs/10.1021/acs.jproteome.6b00034)

* [Global Identification of Protein Post-translational Modifications in a Single-Pass Database Search--J. Proteome Res., 2015, 14 (11), pp 4714–4720](http://pubs.acs.org/doi/abs/10.1021/acs.jproteome.5b00599)

* [A Proteomics Search Algorithm Specifically Designed for High-Resolution Tandem Mass Spectra--J. Proteome Res., 2013, 12 (3), pp 1377–1386](http://pubs.acs.org/doi/abs/10.1021/pr301024c)

## License

The software is currently released under the [GNU GPLv3](http://www.gnu.org/licenses/gpl.txt).

Copyright 2016
