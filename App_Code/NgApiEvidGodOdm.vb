Imports System.Data
Imports System.Web.Configuration
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient
Imports Newtonsoft.Json

Public Class NgApiEvidGodOdm

    Public Sub New()

    End Sub

    Public Sub New(ByVal pEmployeeID As Integer, ByVal pYYYY As Integer)
        Me.EmployeeID = pEmployeeID
        Me.YYYY = pYYYY

        Me.EvidGodOdm = New NgEvidGodOdm

    End Sub

    Public Property EmployeeID As Integer
    Public Property YYYY As Integer

    Public Property EvidGodOdm As NgEvidGodOdm

    Public Sub getEvidGodOdm()

        Dim ConnectionString As String = WebConfigurationManager.ConnectionStrings("MySQLConnection").ConnectionString
        Dim strSQL As String
        Dim DtList As New DataTable

        Dim _oneNgEvidGodOdm As New NgEvidGodOdm
        _oneNgEvidGodOdm.employeeID = Me.EmployeeID
        _oneNgEvidGodOdm.ycurr = Me.YYYY


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim mycmd As New MySql.Data.MySqlClient.MySqlCommand
            Dim myrd As MySqlDataReader

            mycmd.Connection = myconnection

            strSQL = <![CDATA[ 
SELECT ego.IzrDani + ego.IzrDaniPreneseni-ego.IzrDaniAnul AS IzrDani FROM evd_godisnjiodmor ego 
                            WHERE ego.Godina=@YYYY AND ego.EmployeeID=@employeeID;
    ]]>.Value
            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@employeeID", Me.EmployeeID)
            mycmd.Parameters.AddWithValue("@YYYY", Me.YYYY)
            mycmd.Prepare()

            Try
                myrd = mycmd.ExecuteReader()
                While myrd.Read
                    _oneNgEvidGodOdm.ycurr_rjes = myrd.GetInt16(myrd.GetOrdinal("IzrDani"))
                End While
                myrd.Close()
            Catch ex As Exception

            End Try



            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@employeeID", Me.EmployeeID)
            mycmd.Parameters.AddWithValue("@YYYY", (Me.YYYY - 1).ToString)
            mycmd.Prepare()
            Try
                myrd = mycmd.ExecuteReader()
                While myrd.Read
                    _oneNgEvidGodOdm.yprev_rjes = myrd.GetInt16(myrd.GetOrdinal("IzrDani"))
                End While
                myrd.Close()
            Catch ex As Exception

            End Try

            strSQL = <![CDATA[ 
SELECT COUNT(ep.vrijeme_ukupno) AS NumOfDays FROM evd_prisustva ep WHERE ep.employeeID=@employeeID AND 
                ep.sifra_placanja=@sifra_placanja GROUP BY ep.sifra_placanja;
    ]]>.Value

            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@employeeID", Me.EmployeeID)
            mycmd.Parameters.AddWithValue("@sifra_placanja", "GO-" + Me.YYYY.ToString)
            mycmd.Prepare()

            Try
                myrd = mycmd.ExecuteReader()
                While myrd.Read
                    _oneNgEvidGodOdm.ycurr_isk = myrd.GetInt16(myrd.GetOrdinal("NumOfDays"))
                End While
                myrd.Close()
            Catch ex As Exception

            End Try



            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@employeeID", Me.EmployeeID)
            mycmd.Parameters.AddWithValue("@sifra_placanja", "GO-" + (Me.YYYY - 1).ToString)
            mycmd.Prepare()

            Try
                myrd = mycmd.ExecuteReader()
                While myrd.Read
                    _oneNgEvidGodOdm.yprev_isk = myrd.GetInt16(myrd.GetOrdinal("NumOfDays"))
                End While
                myrd.Close()
            Catch ex As Exception

            End Try

        End Using

        Me.EvidGodOdm = _oneNgEvidGodOdm
    End Sub

End Class
