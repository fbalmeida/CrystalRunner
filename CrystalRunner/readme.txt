Syntax for use with Delphi - 


  // Carrega RPT e SQL
  Crpe1.ReportName := 'CRAA001.rpt';

  Crpe1.Connect.ServerName := 'localhost';
  Crpe1.Connect.DatabaseName := 'DBName';
  Crpe1.Connect.UserID := 'sa';
  Crpe1.Connect.Password := '123';

  // Parametros
  Crpe1.ParamFields[0].Name := 'cdCobranca';
  Crpe1.ParamFields[0].CurrentValue := '00032';

  Crpe1.ParamFields[1].Name := 'nrCNPJEmpresa';
  Crpe1.ParamFields[1].CurrentValue := '91.882.596/0001-56';

  Crpe1.ParamFields[2].Name := 'nrCPFCNPJCliente';
  Crpe1.ParamFields[2].CurrentValue := '214.342.140-01';

  Crpe1.Execute;
