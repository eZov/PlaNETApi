Imports System.Web.Configuration
Imports MySql.Data
Imports MySql.Data.MySqlClient
Imports Microsoft.VisualBasic

Public Class ngProtokol

    Public Property Id As Integer
    Public Property RedBr As Integer
    Public Property Protokol As String
    Public Property Godina As Integer
    Public Property OrgJed As String
    Public Property Opis As String

    Public Sub Insert()
        Dim ConnectionString As String = ApiGlobal.domainConnectionString

        Dim mycmd As New MySqlCommand
        mycmd.CommandText = <![CDATA[
INSERT INTO `sfr_protokol` (
  `red_br`,
  `protokol`,
  `godina`,
  `org_jed`,
  `opis`
)
VALUES
  (
    @red_br,
    @protokol,
    @godina,
    @org_jed,
    @opis
  );

    ]]>.Value


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()
            mycmd.Connection = myconnection



            BindParams(mycmd)
            mycmd.Prepare()

            Try
                mycmd.ExecuteNonQuery()
                Id = mycmd.LastInsertedId
            Catch ex As Exception
                Console.WriteLine(ex.Message)
            End Try

        End Using


    End Sub

    Public Sub Update()
        Dim mycmd As New MySqlCommand
        mycmd.CommandText = <![CDATA[
UPDATE
  `sfr_protokol`
SET
  `red_br` = @red_br,
  `protokol` = @protokol,
  `godina` = @godina,
  `org_jed` = @org_jed,
  `opis` = @opis
WHERE `id` = @id;
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
  `sfr_protokol`
WHERE `id` = @id;
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

        mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@id", .DbType = Data.DbType.UInt32, .Value = Id})
        mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@red_br", .DbType = Data.DbType.Int32, .Value = RedBr})
        mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@protokol", .DbType = Data.DbType.String, .Value = Protokol})
        mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@godina", .DbType = Data.DbType.Int32, .Value = Godina})
        mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@org_jed", .DbType = Data.DbType.String, .Value = OrgJed})
        mycmd.Parameters.Add(New MySqlParameter With {.ParameterName = "@opis", .DbType = Data.DbType.String, .Value = Opis})

    End Sub



End Class
