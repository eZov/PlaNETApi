Imports Microsoft.VisualBasic
Imports MySql.Data
Imports System.Data
Imports System.Collections.Generic
Imports System
Imports System.Drawing

Public Class ClsSfrPrisustva

    Public ListSfrPrisustva As New List(Of ListItem)
    Public ListColors As New Dictionary(Of String, Drawing.Color)
    Public ListVrsta As New Dictionary(Of String, String)
    Public ListNaziv As New Dictionary(Of String, String)


    Public Sub New()
        Dim cnData As New MySql.Data.MySqlClient.MySqlConnection
        Dim lcmd As MySql.Data.MySqlClient.MySqlCommand
        Dim ldr As MySql.Data.MySqlClient.MySqlDataReader

        cnData.ConnectionString = ConnectionString
        cnData.Open()



        Dim ListItemTxt As String = ""
        Dim ListItemVal As String = ""

        Dim strSQL As String = "SELECT `Sifra`, `Naziv`, `Vrsta`, `Color`, `ExclList` FROM `sfr_prisustva` ORDER BY `Vrsta` DESC;"

        lcmd = New MySql.Data.MySqlClient.MySqlCommand(strSQL, cnData)
        ldr = lcmd.ExecuteReader
        While ldr.Read
            Try
                ListItemTxt = ldr.Item("Sifra") + "-" + ldr.Item("Naziv")
                ListItemVal = ldr.Item("Sifra")
                ListSfrPrisustva.Add(New ListItem(ListItemTxt, ListItemVal))
                ListColors.Add(ListItemVal, Color.FromName(ldr.Item("Color")))
                ListVrsta.Add(ldr.Item("Sifra"), ldr.Item("Vrsta"))
                ListNaziv.Add(ldr.Item("Sifra"), ldr.Item("Naziv"))
            Catch ex As Exception
                ListItemTxt = ""
                ListItemVal = ""
            End Try

        End While
        ldr.Close()

        cnData.Close()

        ListSfrPrisustva.Add(New ListItem("Brisanje", "DELETE"))
        ListColors.Add("DELETE", Color.FromName("White"))

    End Sub

    Public Sub getSfrPrisustva(ByRef Repeater1 As Repeater)

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim myds As New Data.DataSet

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim strSQL As String = "SELECT `id`, `Sifra`, `Naziv`, `Vrsta`, `Color`, `ExclList` FROM `sfr_prisustva` ORDER BY `Vrsta` DESC, Sifra ASC;"


            myda = New MySqlClient.MySqlDataAdapter(strSQL, myconnection)
            myda.Fill(myds, "SfrPrisustva")

            'Dim cmd As New MySqlClient.MySqlCommand(sqlText, myconnection)
            'myds.Tables("SfrPrisustva").Rows.Add(999, "DELETE", "Brisanje", "", "White", "")

            Repeater1.DataSource = myds.Tables("SfrPrisustva")
            Repeater1.DataBind()

        End Using

    End Sub
End Class
