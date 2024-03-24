Imports System.Web.Configuration
Imports MySql.Data
Imports MySql.Data.MySqlClient
Imports Microsoft.VisualBasic

Public Class ngPerson

    Public Property Sifra As Integer
    Public Property Ime As String
    Public Property ImeRoditelja As String
    Public Property Prezime As String
    Public Property KategorijaUposlenika As String
    Public Property JMBG As String
    Public Property Spol As String
    Public Property Status As String

    Public Sub Insert()
        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString

        Dim mycmd As New MySqlCommand
        mycmd.CommandText = <![CDATA[
INSERT INTO `evd_saradnici` (
  `sifra`,
  `ime`,
  `ime_roditelja`,
  `prezime`,
  `kategorija_uposlenika`,
  `JMBG`,
  `spol`,
  `status`
)
VALUES
  (
    @sifra,
    @ime,
    @ime_roditelja,
    @prezime,
    @kategorija_uposlenika,
    @JMBG,
    @spol,
    @status
  );

    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()
            mycmd.Connection = myconnection



            BindParams(mycmd)
            mycmd.Prepare()

            Try
                mycmd.ExecuteNonQuery()
                'mycmd.LastInsertedId
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using


    End Sub

    Public Sub Update()
        Dim mycmd As New MySqlCommand
        mycmd.CommandText = <![CDATA[
UPDATE
  `evd_saradnici`
SET
  `sifra` = @sifra,
  `ime` = @ime,
  `ime_roditelja` = @ime_roditelja,
  `prezime` = @prezime,
  `kategorija_uposlenika` = @kategorija_uposlenika,
  `JMBG` = @jmbg,
  `spol` = @spol,
  `status` = @status
WHERE `sifra` = @sifra;
  );
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()
            mycmd.Connection = myconnection



            BindParams(mycmd)
            mycmd.Prepare()

            Try
                mycmd.ExecuteNonQuery()
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

    End Sub

    Public Sub Delete()
        Dim mycmd As New MySqlCommand
        mycmd.CommandText = <![CDATA[
DELETE
FROM
  `evd_saradnici`
WHERE `sifra` = @sifra;
    ]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()
            mycmd.Connection = myconnection



            BindParams(mycmd)
            mycmd.Prepare()

            Try
                mycmd.ExecuteNonQuery()
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using

    End Sub

    Public Sub BindParams(ByRef mycmd As MySqlCommand)

        mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@sifra", .DbType = Data.DbType.UInt32, .Value = Me.Sifra})
        mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@ime", .DbType = Data.DbType.String, .Value = Ime})
        mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@ime_roditelja", .DbType = Data.DbType.String, .Value = ImeRoditelja})
        mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@prezime", .DbType = Data.DbType.String, .Value = Prezime})
        mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@kategorija_uposlenika", .DbType = Data.DbType.String, .Value = KategorijaUposlenika})
        mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@jmbg", .DbType = Data.DbType.String, .Value = JMBG})
        mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@spol", .DbType = Data.DbType.String, .Value = Spol})
        mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@status", .DbType = Data.DbType.String, .Value = Status})

    End Sub

End Class
