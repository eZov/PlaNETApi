Imports System.Data
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient

Public Class ClsDropDownList

    Public Sub New()

        DtList = New DataTable

    End Sub

    Public Property LboxList As New List(Of ClsTreeView)
    Public Property DtList As New DataTable
    Public Property DtRow As DataRow

    Public Function getList(Optional ByVal pId As Integer = -1) As DataTable

        Dim myda As MySqlClient.MySqlDataAdapter
        Dim mycmd As MySqlCommand
        Dim _SfrOrgJed As String = " "
        Dim _SfrOrgJedSel As String = " "

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim strSQL As String

            strSQL = <![CDATA[ 
SELECT o.Sifra AS Id, IF(o.Sifra=o.`Sifra_Nadnivo`,'0',o.`Sifra_Nadnivo`) AS ParentId, 
CONCAT(o.Naziv,' (',o.Sifra,')') AS Naziv,1 AS HasChild, NULL AS Expanded,
Opcina_1,  o.`Sifra_Nadnivo` AS SfrNadNivo, o.Naziv AS Naziv1,
o.id AS hidId, 'Poslovni partneri' AS Category
FROM sfr_poslovnipartneri o WHERE o.Sifra<>'0000'
-- ORDER BY o.Naziv
UNION
SELECT o.Sifra AS Id, IF(o.Sifra=o.`Sifra_Nadnivo`,'0',o.`Sifra_Nadnivo`) AS ParentId, 
CONCAT(o.Naziv,' (',o.Sifra,')') AS Naziv,1 AS HasChild, NULL AS Expanded,
Opcina_1,  o.`Sifra_Nadnivo` AS SfrNadNivo, o.Naziv AS Naziv1, 
o.id AS hidId,'Banke' AS Category
FROM sfr_banke o WHERE o.Sifra<>'0000'
ORDER BY Category,Naziv;
]]>.Value

            DtList.Clear()
            myda = New MySqlClient.MySqlDataAdapter(strSQL, myconnection)
            myda.Fill(DtList)


            '
            ' Ako je tabela prazna, izađi...
            '
            If DtList.Rows.Count = 0 Then Return DtList


            '
            ' Ako je pId = -1 odaberi top red iz tabele
            '
            If pId = -1 Then

                Dim filteredTableRow() As DataRow = DtList.Select("ParentId=0")
                For Each er In filteredTableRow
                    pId = er("hidId")
                    Exit For
                Next

            End If

            '
            ' Procesing Expand i Select node...
            '
            mycmd = New MySqlCommand
            mycmd.Connection = myconnection



        End Using


        Return DtList


    End Function

    'Public Function getIdx(ByVal pSfrBanka As String, ByVal pCategory As String) As Integer

    'If pSfrBanka.Length = 0 Then Return -1
    'Dim idx As Integer = -1

    'For Each er In DtList.Rows

    '    idx += 1
    '    If er("Id") = pSfrBanka And er("Category") = pCategory Then
    '        Return idx
    '    End If
    'Next

    'Return -1
    'End Function

    Public Function getIdxById(ByVal pIdBanka As Integer, ByVal pIdPoslPartneri As Integer, ByVal pCategory As String) As Integer

        Dim idx As Integer = -1

        For Each er In DtList.Rows
            idx += 1
            Select Case pCategory
                Case "Banke"
                    If er("hidId") = pIdBanka And er("Category") = pCategory Then
                        Return idx
                    End If

                Case "Poslovni partneri"
                    If er("hidId") = pIdPoslPartneri And er("Category") = pCategory Then
                        Return idx
                    End If

            End Select

        Next

        Return -1
    End Function

    'Public Function getSifra(ByVal pIdx As Integer) As String

    'Dim idx As Integer = -1
    'Dim retVal As String = ""

    'For Each er In DtList.Rows
    '    idx += 1
    '    If idx = pIdx Then
    '        retVal = er("Id")
    '        Return retVal
    '    End If
    'Next

    'Return ""
    'End Function


    Public Function getId(ByVal pIdx As Integer, ByRef pIdBanka As Integer, ByRef pIdPoslPartneri As Integer) As Integer

        Dim idx As Integer = -1
        Dim retVal As Integer = -1

        For Each er In DtList.Rows
            idx += 1
            Select Case er("Category")
                Case "Banke"
                    If idx = pIdx Then
                        retVal = CInt(er("hidId"))
                        pIdPoslPartneri = -1
                        pIdBanka = retVal
                        Return idx
                    End If

                Case "Poslovni partneri"
                    If idx = pIdx Then
                        retVal = CInt(er("hidId"))
                        pIdPoslPartneri = retVal
                        pIdBanka = -1
                        Return idx
                    End If

            End Select

        Next

        Return -1
    End Function
End Class
