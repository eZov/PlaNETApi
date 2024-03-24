Imports System.Data
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient


Public Class ClsEvdObustave

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

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
SELECT eo.`id`,
CONCAT(eo.`Naziv`,' (',IF(eo.`PoslovniPartner`<>1, pp.`Naziv`, b.`Naziv`),')') AS Naziv
FROM `evd_obustave` eo, `sfr_poslovnipartneri` pp, `sfr_banke` b 
WHERE eo.`PoslovniPartner`=pp.`id` AND eo.`Banka`=b.`id` ORDER BY eo.`Naziv`;
]]>.Value

            '
            ' Ako je NULL zamijeni sa -1 (Banka i PoslovniPartner)
            '
            strSQL = <![CDATA[ 
 SELECT CAST(o.id AS CHAR) AS Id,'0' AS ParentId,
CONCAT(o.`Naziv`,' (',IF(o.`PoslovniPartner` IS NULL, b.`Naziv`, pp.`Naziv`),')')  AS Naziv,
1 AS HasChild, NULL AS Expanded,
o.`Sifra` AS SfrNadNivo, o.Naziv AS Naziv1, 
IFNULL(Banka,-1) AS Banka, 
IFNULL(PoslovniPartner,-1) AS PoslovniPartner, Procenat, Iznos, o.Atribut_1, 
TekuciObracun, RacunZaUplatu, IznosUplate, 
o.id AS hidId
FROM `evd_obustave` o LEFT JOIN `sfr_poslovnipartneri` pp ON o.`PoslovniPartner`= pp.`id`
LEFT JOIN  `sfr_banke` b ON o.`Banka`=b.`id`
WHERE o.Sifra<>'0000' 
ORDER BY o.Naziv;
]]>.Value

            'mycmd.CommandText = strSQL.Replace("@Sifra", pSfr)
            mycmd.CommandText = strSQL


            'mycmd.Parameters.Clear()
            'mycmd.Parameters.AddWithValue("@employeeID", EmployeeId)

            DtList.Rows.Clear()
            myda = New MySqlDataAdapter(mycmd)
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

            '
            ' Koristi reference na id umjesto Sifra, jer se u obustavama ne koristi Sifra (ista je za sve redove:9999)
            '
            strSQL = <![CDATA[ 
SELECT so.id AS Sifra, CONCAT(so1.id,',', so2.id) AS sf  
FROM `evd_obustave` so 
LEFT JOIN `evd_obustave` so1 ON so.id=so1.id
LEFT JOIN `evd_obustave` so2 ON so1.id=so2.id
WHERE so.id=@id;
]]>.Value
            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@id", pId)
            mycmd.Prepare()

            Dim rd As MySqlDataReader = mycmd.ExecuteReader()

            While rd.Read
                If rd.IsDBNull(rd.GetOrdinal("sf")) = False Then
                    _SfrOrgJed = rd.GetString(rd.GetOrdinal("sf"))
                End If
                _SfrOrgJedSel = rd.GetString("Sifra")
            End While

            rd.Close()

        End Using


        PopulateLboxList(_SfrOrgJed, _SfrOrgJedSel)

        Return DtList

    End Function

    Private Sub PopulateLboxList(ByVal pSfrOrgJed As String, ByVal pSfrOrgJedSel As String)
        '
        ' Koristi reference na id umjesto Sifra, jer se u obustavama ne koristi Sifra (ista je za sve redove:9999)
        '
        LboxList.Clear()

        Dim _parentId As Integer = 0

        For Each er As DataRow In DtList.Rows
            If er.Item("ParentId") = "" Then
                _parentId = 0
            Else
                _parentId = CInt(er.Item("ParentId"))
            End If

            Dim _SfrOrgJedLoop As String = CInt(er.Item("id")).ToString
            If pSfrOrgJed.Contains(_SfrOrgJedLoop) And pSfrOrgJedSel <> _SfrOrgJedLoop Then
                LboxList.Add(New ClsTreeView With {.Id = er.Item("id"), .ParentId = _parentId, .Naziv = er.Item("Naziv"), .HasChild = "true", .Expanded = True, .Selected = False})
            ElseIf pSfrOrgJedSel = _SfrOrgJedLoop Then
                LboxList.Add(New ClsTreeView With {.Id = er.Item("id"), .ParentId = _parentId, .Naziv = er.Item("Naziv"), .HasChild = "true", .Expanded = False, .Selected = True})
            Else
                LboxList.Add(New ClsTreeView With {.Id = er.Item("id"), .ParentId = _parentId, .Naziv = er.Item("Naziv"), .HasChild = "true", .Expanded = False, .Selected = False})
            End If

        Next

    End Sub

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

    Public Function getIdx(ByVal pId As Integer) As Integer

        Dim idx As Integer = -1

        For Each er In DtList.Rows
            idx += 1
            If er("id") = pId Then
                Return idx
            End If
        Next

        Return -1
    End Function

    Public Function getId(ByVal pIdx As Integer) As Integer

        Dim idx As Integer = -1
        Dim retVal As Integer = -1

        For Each er In DtList.Rows
            idx += 1
            If idx = pIdx Then
                retVal = CInt(er("id"))
                Return retVal
            End If
        Next

        Return -1
    End Function

    Public Sub saveRow(ByRef pDbFields As Hashtable)


        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Try
                mycmd = New MySqlCommand
                mycmd.Connection = myconnection

                Dim strSQL As String = <![CDATA[ 
UPDATE evd_obustave
SET
 Naziv = @Naziv, Iznos = @Iznos, Procenat = @Procenat,
 RacunZaUplatu = @RacunZaUplatu,
 Banka = @Banka, PoslovniPartner = @PoslovniPartner 
WHERE id = @id;
    ]]>.Value


                mycmd.CommandText = strSQL


                mycmd.Parameters.AddWithValue("@id", pDbFields("id"))
                mycmd.Parameters.AddWithValue("@Naziv", pDbFields("Naziv"))
                mycmd.Parameters.AddWithValue("@Iznos", pDbFields("Iznos"))

                mycmd.Parameters.AddWithValue("@Procenat", pDbFields("Procenat"))
                mycmd.Parameters.AddWithValue("@RacunZaUplatu", pDbFields("RacunZaUplatu"))

                mycmd.Parameters.AddWithValue("@Banka", pDbFields("Banka"))
                mycmd.Parameters.AddWithValue("@PoslovniPartner", pDbFields("PoslovniPartner"))

                mycmd.Prepare()

                mycmd.ExecuteNonQuery()
            Catch ex As Exception
                ex.ToString()
            End Try


        End Using

    End Sub

    Public Sub insRow(ByRef pDbFields As Hashtable, Optional ByRef pRowId As Integer = -1)


        Dim mycmd As MySqlCommand

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Try
                mycmd = New MySqlCommand
                mycmd.Connection = myconnection

                Dim strSQL As String = <![CDATA[ 
INSERT INTO evd_obustave (
  Sifra, Naziv, Banka, PoslovniPartner, Procenat, Iznos, RacunZaUplatu
)
VALUES
  (
    @Sifra, @Naziv, @Banka, @PoslovniPartner, @Procenat, @Iznos, @RacunZaUplatu
  );
    ]]>.Value


                mycmd.CommandText = strSQL


                mycmd.Parameters.AddWithValue("@id", pDbFields("id"))
                mycmd.Parameters.AddWithValue("@Sifra", "9999")
                mycmd.Parameters.AddWithValue("@Naziv", pDbFields("Naziv"))
                mycmd.Parameters.AddWithValue("@Iznos", pDbFields("Iznos"))

                mycmd.Parameters.AddWithValue("@Procenat", pDbFields("Procenat"))
                mycmd.Parameters.AddWithValue("@RacunZaUplatu", pDbFields("RacunZaUplatu"))

                mycmd.Parameters.AddWithValue("@Banka", pDbFields("Banka"))
                mycmd.Parameters.AddWithValue("@PoslovniPartner", pDbFields("PoslovniPartner"))

                mycmd.Prepare()

                mycmd.ExecuteNonQuery()

                pRowId = mycmd.LastInsertedId

            Catch ex As Exception
                ex.ToString()
            End Try


        End Using


    End Sub

End Class
