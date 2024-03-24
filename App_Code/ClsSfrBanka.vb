Imports System.Data
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient

Public Class ClsSfrBanka

    Private cDtBanka As DataTable

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

            Dim strSQL As String = <![CDATA[ 
SELECT o.Sifra AS Id, IF(o.Sifra=o.`Sifra_Nadnivo`,'0',o.`Sifra_Nadnivo`) AS ParentId, 
CONCAT(o.Naziv,' (',o.Sifra,')') AS Naziv,1 AS HasChild, NULL AS Expanded,
Opcina_1,  o.`Sifra_Nadnivo` AS SfrNadNivo, o.Naziv AS Naziv1, o.Opis,
Mjesto, Adresa, PTT_Broj,
Banka_1, Banka_2, BankaRacun_1, 
o.id AS hidId,
cb_Sifra
FROM sfr_banke o WHERE o.Sifra<>'0000'
ORDER BY o.Naziv;
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

            strSQL = <![CDATA[ 
SELECT so.`Sifra`, CONCAT(so1.`Sifra`,',', so2.`Sifra`,',', so3.`Sifra`,',',  so4.`Sifra` ,',',  so5.`Sifra`) AS sf  FROM `sfr_banke` so 
LEFT JOIN `sfr_banke` so1 ON so.`Sifra_Nadnivo`=so1.`Sifra`
LEFT JOIN `sfr_banke` so2 ON so1.`Sifra_Nadnivo`=so2.`Sifra`
LEFT JOIN `sfr_banke` so3 ON so2.`Sifra_Nadnivo`=so3.`Sifra`
LEFT JOIN `sfr_banke` so4 ON so3.`Sifra_Nadnivo`=so4.`Sifra`
LEFT JOIN `sfr_banke` so5 ON so4.`Sifra_Nadnivo`=so5.`Sifra`
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

    Public Function getIdx(Optional ByVal pSfrBanka As String = "000000") As Integer

        If pSfrBanka.Length = 0 Then Return -1
        Dim idx As Integer = -1

        For Each er In DtList.Rows

            idx += 1
            If er("Id") = pSfrBanka Then
                Return idx
            End If
        Next

        Return -1
    End Function

    Public Function getIdxById(Optional ByVal pIdBanka As Integer = -1) As Integer

        If pIdBanka = -1 Then Return -1
        Dim idx As Integer = -1

        For Each er In DtList.Rows
            idx += 1
            If er("hidId") = pIdBanka Then
                Return idx
            End If
        Next

        Return -1
    End Function

    Public Function getSifra(ByVal pIdx As Integer) As String

        Dim idx As Integer = -1
        Dim retVal As String = ""

        For Each er In DtList.Rows
            idx += 1
            If idx = pIdx Then
                retVal = er("Id")
                Return retVal
            End If
        Next

        Return ""
    End Function


    Public Function getId(ByVal pIdx As Integer) As Integer

        Dim idx As Integer = -1
        Dim retVal As Integer = -1

        For Each er In DtList.Rows
            idx += 1
            If idx = pIdx Then
                retVal = CInt(er("hidId"))
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
UPDATE sfr_banke
SET
 Sifra = @Sifra, `Sifra_Nadnivo` = @Sifra_Nadnivo, Naziv = @Naziv, Opis = @Opis,
 Opcina_1 = @Opcina_1,
 Banka_1 = @Banka_1, Banka_2 = @Banka_2,  
  BankaRacun_1 = @BankaRacun_1, 
  Mjesto = @Mjesto, Adresa = @Adresa, PTT_Broj = @PTT_Broj

WHERE id = @id;
    ]]>.Value


                mycmd.CommandText = strSQL
    

                mycmd.Parameters.AddWithValue("@id", pDbFields("id"))
                mycmd.Parameters.AddWithValue("@Sifra", pDbFields("Sifra"))
                mycmd.Parameters.AddWithValue("@Sifra_Nadnivo", pDbFields("Sifra_Nadnivo"))

                mycmd.Parameters.AddWithValue("@Naziv", pDbFields("Naziv"))
                mycmd.Parameters.AddWithValue("@Opis", pDbFields("Opis"))

                mycmd.Parameters.AddWithValue("@Opcina_1", pDbFields("Opcina_1"))
                mycmd.Parameters.AddWithValue("@Banka_1", pDbFields("Banka_1"))
                mycmd.Parameters.AddWithValue("@Banka_2", pDbFields("Banka_2"))

                mycmd.Prepare()

                Dim _cbSifra As String = CStr(Me.rowDetails(pDbFields("id"), "cb_sifra"))
                If CStr(pDbFields("BankaRacun_1")).Length > 0 AndAlso CStr(pDbFields("BankaRacun_1")).Substring(0, 3) <> _cbSifra Then
                    pDbFields("BankaRacun_1") = ""
                End If

                mycmd.Parameters.AddWithValue("@BankaRacun_1", pDbFields("BankaRacun_1"))
                'mycmd.Parameters.AddWithValue("@BankaRacun_2", pDbFields("BankaRacun_2"))

                mycmd.Parameters.AddWithValue("@Adresa", pDbFields("Adresa"))
                mycmd.Parameters.AddWithValue("@Mjesto", pDbFields("Mjesto"))
                mycmd.Parameters.AddWithValue("@PTT_Broj", pDbFields("PTT_Broj"))

                mycmd.ExecuteNonQuery()


                '
                ' Ažuriraj cb_Sifra za sve banke u grani, i sve redove u tabeli
                '
                strSQL = <![CDATA[ 
UPDATE `sfr_banke` sb0,`sfr_banke` sb1,`sfr_banke` sb2,`sfr_banke` sb3,`sfr_banke` sb4,`sfr_banke` sb5
SET sb0.`cb_Sifra`=sb5.`cb_Sifra`
WHERE sb0.`Sifra_Nadnivo`=sb1.`Sifra`
AND sb1.`Sifra_Nadnivo`=sb2.`Sifra` 
AND sb2.`Sifra_Nadnivo`=sb3.`Sifra` 
AND sb3.`Sifra_Nadnivo`=sb4.`Sifra` 
AND sb4.`Sifra_Nadnivo`=sb5.`Sifra`; 
    ]]>.Value


                mycmd.CommandText = strSQL

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
INSERT INTO sfr_banke (
 Sifra, `Sifra_Nadnivo`, Naziv, Opis, Opcina_1, Banka_1, Banka_2,  
 BankaRacun_1, Mjesto, Adresa, PTT_Broj
)
VALUES
  (
   @Sifra, @Sifra_Nadnivo, @Naziv, @Opis, @Opcina_1, @Banka_1, @Banka_2, 
   @BankaRacun_1, @Mjesto, @Adresa, @PTT_Broj
  );
    ]]>.Value


                mycmd.CommandText = strSQL
    

                'mycmd.Parameters.AddWithValue("@id", pDbFields("id"))
                mycmd.Parameters.AddWithValue("@Sifra", pDbFields("Sifra"))
                mycmd.Parameters.AddWithValue("@Sifra_Nadnivo", pDbFields("Sifra_Nadnivo"))

                mycmd.Parameters.AddWithValue("@Naziv", pDbFields("Naziv"))
                mycmd.Parameters.AddWithValue("@Opis", pDbFields("Opis"))

                mycmd.Parameters.AddWithValue("@Opcina_1", pDbFields("Opcina_1"))
                mycmd.Parameters.AddWithValue("@Banka_1", pDbFields("Banka_1"))
                mycmd.Parameters.AddWithValue("@Banka_2", pDbFields("Banka_2"))

                'mycmd.Parameters.AddWithValue("@Obracun", pDbFields("Obracun"))
                'mycmd.Parameters.AddWithValue("@Uposlenici", pDbFields("Uposlenici"))

                'mycmd.Parameters.AddWithValue("@PDVBroj", pDbFields("PDVBroj"))
                'mycmd.Parameters.AddWithValue("@BrojStatisticki", pDbFields("BrojStatisticki"))
                'mycmd.Parameters.AddWithValue("@BrojDjelovodni", pDbFields("BrojDjelovodni"))

                mycmd.Parameters.AddWithValue("@BankaRacun_1", pDbFields("BankaRacun_1"))
                'mycmd.Parameters.AddWithValue("@BankaRacun_2", pDbFields("BankaRacun_2"))

                mycmd.Parameters.AddWithValue("@Adresa", pDbFields("Adresa"))
                mycmd.Parameters.AddWithValue("@Mjesto", pDbFields("Mjesto"))
                mycmd.Parameters.AddWithValue("@PTT_Broj", pDbFields("PTT_Broj"))

                mycmd.Prepare()
                mycmd.ExecuteNonQuery()

                pRowId = mycmd.LastInsertedId


                mycmd.Parameters.Clear()

                mycmd.Parameters.AddWithValue("@id", pRowId)
                mycmd.Parameters.AddWithValue("@ids", pRowId.ToString)

                If pDbFields("Sifra_Nadnivo") = "NEMASIFRU" Then pDbFields("Sifra_Nadnivo") = pRowId.ToString
                mycmd.Parameters.AddWithValue("@Sifra_Nadnivo", pDbFields("Sifra_Nadnivo"))


                strSQL = <![CDATA[ 
UPDATE sfr_banke 
SET Sifra=@ids, Sifra_Nadnivo= @Sifra_Nadnivo
WHERE id = @id
    ]]>.Value

                mycmd.Prepare()

                mycmd.CommandText = strSQL

                mycmd.ExecuteNonQuery()

            Catch ex As Exception
                ex.ToString()
            End Try


        End Using


    End Sub

    ''' <summary>
    ''' Vrat Id na bazi CbŠifre sa preddefinisane banke
    ''' </summary>
    ''' <param name="pCbSifra"></param>
    ''' <returns></returns>
    Public Function getCbSifraId(ByVal pCbSifra As String) As Integer


        Dim mycmd As MySqlCommand
        Dim strSQL As String

        Dim _cbSifraId As Integer = -1

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            '
            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            strSQL = <![CDATA[ 
SELECT sb.`id` FROM `sfr_banke` sb WHERE sb.`cb_Sifra`=@cb_Sifra AND sb.`Sifra`=sb.`Sifra_Nadnivo`;    
]]>.Value
            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@cb_Sifra", pCbSifra)
            mycmd.Prepare()

            Dim rd As MySqlDataReader = mycmd.ExecuteReader()

            While rd.Read
                If rd.IsDBNull(rd.GetOrdinal("id")) = False Then
                    _cbSifraId = rd.GetString(rd.GetOrdinal("id"))
                End If
            End While

            rd.Close()

        End Using


        Return _cbSifraId

    End Function

    ''' <summary>
    ''' Vraća TRUE ako je za dato Id jedano CbŠifri
    ''' </summary>
    ''' <param name="pCbSifra"></param>
    ''' <param name="pId"></param>
    ''' <returns></returns>
    Public Function checkCbSifra(ByVal pCbSifra As String, ByVal pId As Integer) As Boolean


        Dim mycmd As MySqlCommand
        Dim strSQL As String

        Dim _cbSifraOk As Boolean = False

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            '
            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            strSQL = <![CDATA[ 
SELECT sb.`id` FROM `sfr_banke` sb WHERE sb.`cb_Sifra`=@cb_Sifra AND sb.`id`=@id;    
]]>.Value
            mycmd.CommandText = strSQL


            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@id", pId)
            mycmd.Parameters.AddWithValue("@cb_Sifra", pCbSifra)
            mycmd.Prepare()

            Dim rd As MySqlDataReader = mycmd.ExecuteReader()

            While rd.Read
                If rd.IsDBNull(rd.GetOrdinal("id")) = False Then
                    _cbSifraOk = rd.GetValue(rd.GetOrdinal("id"))
                End If
            End While

            rd.Close()

        End Using


        Return _cbSifraOk

    End Function

    ''' <summary>
    ''' Za dato Id, vraća kolonu sa nazivom: pColName
    ''' </summary>
    ''' <param name="pId"></param>
    ''' <param name="pColName"></param>
    ''' <returns></returns>
    Public Function rowDetails(ByVal pId As Integer, ByVal pColName As String) As Object

        If pId < 0 Or pColName.Length = 0 Then Return Nothing

        For Each er In DtList.Rows
            If er("hidId") = pId Then
                DtRow = er
                Exit For
            End If
        Next

        If DtRow IsNot Nothing Then
            Return DtRow.Item(pColName)
        Else
            Return Nothing
        End If

    End Function
End Class
