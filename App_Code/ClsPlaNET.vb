Imports System.Data
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient
Public Class ClsPlaNET

    Public Sub New()

        getPaket()

    End Sub

    Public Property Paket As String
    Public Property Limit As String
    Public Property Obustave As Boolean = False  ' True(1) - Obustave postoje, False(0) - nema obustava
    Public Property XmlReport As Boolean = False
    Public Property GodOdm As Boolean = False
    Public Property GodOdmRjes As Boolean = False
    Public Property Evidencija As Boolean = False
    Public Property DnRada As Boolean = False

    Public Sub getPaket()

        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
SELECT  sc.`adm_parameter`,sc.`adm_value` FROM `sys_configuration` sc WHERE sc.`adm_parameter` LIKE 'planet%';
]]>.Value
            'mycmd.CommandText = strSQL.Replace("@Sifra", pSfr)
            mycmd.CommandText = strSQL

            'mycmd.Parameters.AddWithValue("@DocVrsta", pDocVrsta)

            Dim rd As MySqlDataReader = mycmd.ExecuteReader()
            Dim _admvalue As String

            While rd.Read
                Select Case rd("adm_parameter")
                    Case "planet.limit"
                        Limit = rd.GetString("adm_value")
                    Case "planet.paket"
                        Paket = rd.GetString("adm_value")
                    Case "planet.obustave"
                        _admvalue = rd.GetString("adm_value")
                        If _admvalue = "1" Then Obustave = True
                    Case "planet.xmlreport"
                        _admvalue = rd.GetString("adm_value")
                        If _admvalue = "1" Then XmlReport = True
                    Case "planet.gododm"
                        _admvalue = rd.GetString("adm_value")
                        If _admvalue = "1" Then GodOdm = True
                    Case "planet.gododmrjes"
                        _admvalue = rd.GetString("adm_value")
                        If _admvalue = "1" Then GodOdmRjes = True
                    Case "planet.evidencija"
                        _admvalue = rd.GetString("adm_value")
                        If _admvalue = "1" Then Evidencija = True

                    Case "planet.dnrada"
                        _admvalue = rd.GetString("adm_value")
                        If _admvalue = "1" Then DnRada = True

                    Case Else

                End Select


            End While

            rd.Close()

        End Using



    End Sub

End Class
