Imports System.Data
Imports System.Drawing
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient

Public Class ClsSfrEvidencije
    Private cSfrEvidencije As New Dictionary(Of String, ClsSfrEvidItem)


    Public ReadOnly Property Lista() As Dictionary(Of String, ClsSfrEvidItem)
        Get
            Return cSfrEvidencije
        End Get
    End Property

    Public Sub AddSfrEvid(ByRef pSfrEvid As ClsSfrEvidItem)
        cSfrEvidencije.Add(pSfrEvid.Sifra, pSfrEvid)
    End Sub

    Public Sub RemoveSfrEvid(ByRef pSfrEvid As ClsSfrEvidItem)
        cSfrEvidencije.Remove(pSfrEvid.Sifra)
    End Sub

    Public Sub LoadDbEvid()

        Dim sqlText As String
        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()
            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            sqlText = <![CDATA[ 
SELECT `id`, `Sifra`, `Naziv`, `Vrsta`, `Color`, `ExclList` FROM `sfr_prisustva`;
]]>.Value


            mycmd.CommandText = sqlText

            Dim dbRead As MySqlClient.MySqlDataReader
            Dim newSfrEvidItem As ClsSfrEvidItem

            dbRead = mycmd.ExecuteReader

            Do While dbRead.Read
                newSfrEvidItem = New ClsSfrEvidItem

                newSfrEvidItem.UniqueId = ClsDatabase.DBGetInteger(dbRead("id"))
                newSfrEvidItem.Sifra = ClsDatabase.DBGetText(dbRead("Sifra"))
                newSfrEvidItem.ExclusionList = ClsDatabase.DBGetText(dbRead("ExclList"))
                newSfrEvidItem.Naziv = ClsDatabase.DBGetText(dbRead("Naziv"))
                newSfrEvidItem.LabelColor = Color.FromName(ClsDatabase.DBGetText(dbRead("Color")))
                Me.AddSfrEvid(newSfrEvidItem)

                Select Case newSfrEvidItem.Sifra
                    Case "GO-OVAGOD"
                        newSfrEvidItem.Sifra = newSfrEvidItem.Sifra.Replace("OVAGOD", Now.Year.ToString)
                        newSfrEvidItem.Naziv = newSfrEvidItem.Naziv.Replace("OVAGOD", Now.Year.ToString)
                        newSfrEvidItem.ExclusionList = newSfrEvidItem.ExclusionList.Replace("OVAGOD", Now.Year.ToString)
                        newSfrEvidItem.ExclusionList = newSfrEvidItem.ExclusionList.Replace("PROSLAGOD", (Now.Year - 1).ToString)
                    Case "GO-PROSLAGOD"
                        newSfrEvidItem.Sifra = newSfrEvidItem.Sifra.Replace("PROSLAGOD", (Now.Year - 1).ToString)
                        newSfrEvidItem.Naziv = newSfrEvidItem.Naziv.Replace("PROSLAGOD", (Now.Year - 1).ToString)
                        newSfrEvidItem.ExclusionList = newSfrEvidItem.ExclusionList.Replace("OVAGOD", Now.Year.ToString)
                        newSfrEvidItem.ExclusionList = newSfrEvidItem.ExclusionList.Replace("PROSLAGOD", (Now.Year - 1).ToString)

                    Case Else
                        newSfrEvidItem.ExclusionList = newSfrEvidItem.ExclusionList.Replace("OVAGOD", Now.Year.ToString)
                        newSfrEvidItem.ExclusionList = newSfrEvidItem.ExclusionList.Replace("PROSLAGOD", (Now.Year - 1).ToString)

                End Select

            Loop
            dbRead.Close()
            dbRead = Nothing


        End Using



    End Sub
End Class



Public Class ClsSfrEvidItem

    Public UniqueId As Integer

    Public Sifra As String
    Public Sifra2 As String
    Public Naziv As String
    Public LabelColor As System.Drawing.Color
    Public ButtonEvid As Button

    Public ExclusionList As String

    Public DBAction As Integer = 0  ' -1 brisanje, +1 insert ili update

    Public Sub Clear()
        Sifra = ""
        Sifra2 = ""
        Naziv = ""
        LabelColor = Nothing
        ButtonEvid = Nothing
        ExclusionList = ""

    End Sub


End Class
