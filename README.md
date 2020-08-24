# OracleDataImport

## Projects
### DataImportLib
DataImportLib is the project which generates the library containing all of the functions required to perform imports to the Data Warehouse staging area from Oracle data sources
### DataImportRunner
DataImportRunner is a project containing a CLI application which will execute all source table imports specified in the metadata database. This project is intended to be run by scheduling software.
### OracleDataImport
OracleDataImport is the project containing a windows forms application allowing execution of the Data Imports interactively,  this project also contains the tool for generated encrypted passwords for storage in the configuration database.
