﻿Imports Syncfusion.Schedule
Imports System.Data
Imports Microsoft.VisualBasic
Imports MySql.Data
Imports MySql.Data.MySqlClient

Public Class ClsGOEvidencije

    Public EmployeeId As Integer = 0
    Public EmployeeName As String

    'sys configuration parametri
    Public GO_InitGodina As Integer
    Public GO_AllowAll As Integer = 0

    Public GO_CurrYearPreostalo As Integer = 0
    Public GO_PrevYearPreostalo As Integer = 0
    Public GO_CurrYearIskoristeno As Integer = 0

    Public EvListaSifri As New Dictionary(Of String, ClsEvidItem)

    ' lista mogućih vrsta plaćanja - nevezano za uposlenika
    Public EvSfrLista As New List(Of ClsEvidItem)

    ' stvarni podaci - vezani za uposlenika
    Public EvEvidMasterList As New List(Of ClsEvidItem)
    Public EvSDPMasterList As New SimpleScheduleAppointmentList

    Private cDrzPraznici As New List(Of Date)

    Public Sub New(ByVal pGodina As Integer)
        LoadDrzPraznici(pGodina)
    End Sub

    Public ReadOnly Property ListaSifri As Dictionary(Of String, ClsEvidItem)
        Get
            Return EvListaSifri
        End Get
    End Property

    Public ReadOnly Property DrzPraznici() As List(Of Date)
        Get
            Return cDrzPraznici
        End Get
    End Property

    Public Function IsPraznik(ByVal pDate As Date) As Boolean

        For Each el As Date In cDrzPraznici
            If pDate = el Then
                Return True
            End If
        Next

        Return False

    End Function

    Public Function ExclusionList(ByVal pSifra As String) As String
        For Each el As ClsEvidItem In EvSfrLista
            If el.Sifra = pSifra Then
                Return el.ExclusionList
            End If
        Next

        Return ""
    End Function

    Public Sub GetEvidencije(ByVal evMasterList As SimpleScheduleAppointmentList)
        Dim dbRead As MySqlClient.MySqlDataReader
        Dim mycmd As MySqlCommand

        Dim txtOrgJed As String = ""
        Dim txtParOrgJed As String = ""
        Dim txtFootOrgJed As String = ""
        Dim txtSifra As String = ""

        EvEvidMasterList.Clear()


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
SELECT ep.`evd_prisustva_id`, ep.`employeeID`, ep.`sifra_placanja`, 
                            ep.`datum`, ep.`vrijeme_od`, ep.`vrijeme_do`, ep.`vrijeme_ukupno`
                            FROM `evd_prisustva` ep WHERE ep.`employeeID`=?employeeID;
]]>.Value
            strSQL = strSQL.Replace("?employeeID", EmployeeId)

            mycmd.CommandText = strSQL


            Dim _Working As Integer = 0

            dbRead = mycmd.ExecuteReader
            Try
                Do While dbRead.Read
                    Dim newItem As New ClsEvidItem

                    newItem.EmployeeId = EmployeeId

                    newItem.UniqueId = dbRead("evd_prisustva_id")
                    newItem.Sifra = dbRead("sifra_placanja")
                    newItem.Datum = ClsDatabase.DBGetDate(dbRead("datum"))
                    newItem.VrijemeOd = newItem.Datum.Add(dbRead("vrijeme_od"))
                    newItem.VrijemeDo = newItem.Datum.Add(dbRead("vrijeme_do"))
                    newItem.VrijemeUk = dbRead("vrijeme_ukupno")

                    EvEvidMasterList.Add(newItem)

                    Dim item As ScheduleAppointment = TryCast(evMasterList.NewScheduleAppointment(), ScheduleAppointment)

                    item.StartTime = newItem.Datum.AddHours(8)
                    item.EndTime = item.StartTime.AddHours(8)

                    item.Subject = newItem.Sifra
                    item.Content = newItem.Sifra

                    If EvListaSifri.ContainsKey(newItem.Sifra) Then
                        item.LabelValue = EvListaSifri.Item(newItem.Sifra).LabelColor
                    End If
                    'item.LabelValue = evLblCol
                    item.LocationValue = ""

                    item.UniqueID = newItem.UniqueId

                    item.MarkerValue = 2
                    item.Dirty = False

                    evMasterList.Add(item)
                Loop
            Catch ex As Exception
                ex.ToString()
            End Try
            dbRead.Close()


        End Using
    End Sub

    Public Sub GetEvidencije(ByVal evMasterList As SimpleScheduleAppointmentList, ByVal pGodina As Integer)

        Dim dbRead As MySqlClient.MySqlDataReader
        Dim mycmd As MySqlCommand

        Dim txtOrgJed As String = ""
        Dim txtParOrgJed As String = ""
        Dim txtFootOrgJed As String = ""
        Dim txtSifra As String = ""

        EvEvidMasterList.Clear()




        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
SELECT ep.`evd_prisustva_id`, ep.`employeeID`, ep.`sifra_placanja`, 
                            ep.`datum`, ep.`vrijeme_od`, ep.`vrijeme_do`, ep.`vrijeme_ukupno`
                            FROM `evd_prisustva` ep WHERE ep.`employeeID`=?employeeID AND YEAR(ep.datum)=?godina;
]]>.Value
            strSQL = strSQL.Replace("?employeeID", EmployeeId)

            mycmd.CommandText = strSQL


            Dim _Working As Integer = 0

            dbRead = mycmd.ExecuteReader
            Try
                Do While dbRead.Read
                    Dim newItem As New ClsEvidItem

                    newItem.EmployeeId = EmployeeId

                    newItem.UniqueId = dbRead("evd_prisustva_id")
                    newItem.Sifra = dbRead("sifra_placanja")
                    newItem.Datum = ClsDatabase.DBGetDate(dbRead("datum"))
                    newItem.VrijemeOd = newItem.Datum.Add(dbRead("vrijeme_od"))
                    newItem.VrijemeDo = newItem.Datum.Add(dbRead("vrijeme_do"))
                    newItem.VrijemeUk = dbRead("vrijeme_ukupno")

                    EvEvidMasterList.Add(newItem)

                    Dim item As ScheduleAppointment = TryCast(evMasterList.NewScheduleAppointment(), ScheduleAppointment)

                    item.StartTime = newItem.Datum.AddHours(8)
                    item.EndTime = item.StartTime.AddHours(8)

                    item.Subject = newItem.Sifra
                    item.Content = newItem.Sifra

                    If EvListaSifri.ContainsKey(newItem.Sifra) Then
                        item.LabelValue = EvListaSifri.Item(newItem.Sifra).LabelColor
                    End If
                    'item.LabelValue = evLblCol
                    item.LocationValue = ""

                    item.UniqueID = newItem.UniqueId

                    item.MarkerValue = 2
                    item.Dirty = False

                    evMasterList.Add(item)
                Loop
            Catch ex As Exception
                ex.ToString()
            End Try
            dbRead.Close()

        End Using
    End Sub

    Public Sub GetEvidencijeGoRjes(ByVal evMasterList As SimpleScheduleAppointmentList)
        Dim dbRead As MySqlClient.MySqlDataReader
        Dim mycmd As MySqlCommand


        Dim txtOrgJed As String = ""
        Dim txtParOrgJed As String = ""
        Dim txtFootOrgJed As String = ""
        Dim txtSifra As String = ""

        EvEvidMasterList.Clear()


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
SELECT (@cnt =@cnt + 1) AS evd_prisustva_id,
ego.`employeeID`, 
CONCAT('GO-',ego.`godina`) AS sifra_placanja ,
c.`dt` AS datum,
TIME('08:00:00') AS vrijeme_od,
TIME('16:00:00') AS vrijeme_do,
TIME('08:00:00') AS vrijeme_ukupno   
FROM `evd_godisnjiodmor_plan` ego, `sfr_calendar` c,
`evd_godisnjiodmor` eg
 CROSS JOIN (SELECT @cnt = 0) AS dummy
WHERE ego.`employeeID`=?employeeID 
AND c.`dt`>=ego.`startDate` AND c.`dt`<=ego.`endDate` 
AND c.`isWeekday`=1 AND c.`isHoliday`=0
AND eg.`Godina`=ego.`godina` AND eg.`employeeID`=ego.`employeeID` AND eg.`rjesenje_status`<>0
ORDER BY c.`dt`;
]]>.Value
            strSQL = strSQL.Replace("?employeeID", EmployeeId)

            mycmd.CommandText = strSQL

            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@cnt", 0)
            mycmd.Prepare()

            Dim _Working As Integer = 0

            dbRead = mycmd.ExecuteReader
            Try
                Do While dbRead.Read
                    Dim newItem As New ClsEvidItem

                    newItem.EmployeeId = EmployeeId

                    newItem.UniqueId = dbRead("evd_prisustva_id")
                    newItem.Sifra = dbRead("sifra_placanja")
                    newItem.Datum = ClsDatabase.DBGetDate(dbRead("datum"))
                    newItem.VrijemeOd = newItem.Datum.Add(dbRead("vrijeme_od"))
                    newItem.VrijemeDo = newItem.Datum.Add(dbRead("vrijeme_do"))
                    newItem.VrijemeUk = dbRead("vrijeme_ukupno")

                    EvEvidMasterList.Add(newItem)

                    Dim item As ScheduleAppointment = TryCast(evMasterList.NewScheduleAppointment(), ScheduleAppointment)

                    item.StartTime = newItem.Datum.AddHours(8)
                    item.EndTime = item.StartTime.AddHours(8)

                    item.Subject = newItem.Sifra
                    item.Content = newItem.Sifra

                    If EvListaSifri.ContainsKey(newItem.Sifra) Then
                        item.LabelValue = EvListaSifri.Item(newItem.Sifra).LabelColor
                    End If
                    'item.LabelValue = evLblCol
                    item.LocationValue = ""

                    item.UniqueID = newItem.UniqueId

                    item.MarkerValue = 2
                    item.Dirty = False

                    evMasterList.Add(item)
                Loop
            Catch ex As Exception
                ex.ToString()
            End Try
            dbRead.Close()


        End Using
    End Sub

    Public Sub GetEvidencijeGoRjes(ByVal evMasterList As SimpleScheduleAppointmentList, ByVal pGodina As Integer)
        Dim dbRead As MySqlClient.MySqlDataReader
        Dim mycmd As MySqlCommand

        Dim txtOrgJed As String = ""
        Dim txtParOrgJed As String = ""
        Dim txtFootOrgJed As String = ""
        Dim txtSifra As String = ""

        EvEvidMasterList.Clear()

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
SELECT (@cnt =@cnt + 1) AS evd_prisustva_id,
ego.`employeeID`, 
CONCAT('GO-',ego.`godina`) AS sifra_placanja ,
c.`dt` AS datum,
TIME('08:00:00') AS vrijeme_od,
TIME('16:00:00') AS vrijeme_do,
TIME('08:00:00') AS vrijeme_ukupno   
FROM `evd_godisnjiodmor_plan` ego, `sfr_calendar` c
 CROSS JOIN (SELECT @cnt = 0) AS dummy
WHERE ego.`employeeID`=?employeeID 
AND c.`dt`>=ego.`startDate` AND c.`dt`<=ego.`endDate` 
AND c.`y`=?godina 
ORDER BY c.`dt`;
]]>.Value
            strSQL = strSQL.Replace("?employeeID", EmployeeId)
            strSQL = strSQL.Replace("?godina", pGodina)

            mycmd.CommandText = strSQL

            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@cnt", 0)

            mycmd.Prepare()

            Dim _Working As Integer = 0

            dbRead = mycmd.ExecuteReader
            Try
                Do While dbRead.Read
                    Dim newItem As New ClsEvidItem

                    newItem.EmployeeId = EmployeeId

                    newItem.UniqueId = dbRead("evd_prisustva_id")
                    newItem.Sifra = dbRead("sifra_placanja")
                    newItem.Datum = ClsDatabase.DBGetDate(dbRead("datum"))
                    newItem.VrijemeOd = newItem.Datum.Add(dbRead("vrijeme_od"))
                    newItem.VrijemeDo = newItem.Datum.Add(dbRead("vrijeme_do"))
                    newItem.VrijemeUk = dbRead("vrijeme_ukupno")

                    EvEvidMasterList.Add(newItem)

                    Dim item As ScheduleAppointment = TryCast(evMasterList.NewScheduleAppointment(), ScheduleAppointment)

                    item.StartTime = newItem.Datum.AddHours(8)
                    item.EndTime = item.StartTime.AddHours(8)

                    item.Subject = newItem.Sifra
                    item.Content = newItem.Sifra

                    If EvListaSifri.ContainsKey(newItem.Sifra) Then
                        item.LabelValue = EvListaSifri.Item(newItem.Sifra).LabelColor
                    End If
                    'item.LabelValue = evLblCol
                    item.LocationValue = ""

                    item.UniqueID = newItem.UniqueId

                    item.MarkerValue = 2
                    item.Dirty = False

                    evMasterList.Add(item)
                Loop
            Catch ex As Exception
                ex.ToString()
            End Try
            dbRead.Close()


        End Using
    End Sub

    Public Sub SaveEvidencije()

        For Each el As ClsEvidItem In EvSfrLista

            Select Case el.DBAction
                Case -1
                    el.DBRemoveMe()

                Case 0
                    ' Do Nothing

                Case +1
                    If el.UniqueId > 0 Then
                        el.DBUpdateMe()
                    Else
                        el.DBAddMe()
                    End If

                Case Else
                    ' Do Nothing

            End Select
        Next

    End Sub

    Public Sub AddEvidencija(ByVal item As ScheduleAppointment)
        Dim evItem As New ClsEvidItem

        evItem.EmployeeId = EmployeeId

        evItem.DBAction = +1
        evItem.UniqueId = item.UniqueID
        evItem.VrijemeOd = item.StartTime
        evItem.VrijemeDo = item.EndTime
        evItem.VrijemeUk = item.EndTime.Subtract(item.StartTime)
        evItem.Datum = item.StartTime.Date
        evItem.Sifra = item.Subject

        EvEvidMasterList.Add(evItem)

        evItem.DBAddMe()
        item.UniqueID = evItem.UniqueId

    End Sub

    Public Sub RemoveEvidencija(ByVal item As ScheduleAppointment)
        Dim evItem As New ClsEvidItem

        evItem.EmployeeId = EmployeeId

        evItem.DBAction = -1
        evItem.UniqueId = item.UniqueID
        evItem.VrijemeOd = item.StartTime
        evItem.VrijemeDo = item.EndTime
        evItem.VrijemeUk = item.EndTime.Subtract(item.StartTime)
        evItem.Datum = item.StartTime.Date
        evItem.Sifra = item.Subject

        For Each el As ClsEvidItem In EvEvidMasterList
            If el.UniqueId = evItem.UniqueId Then
                EvEvidMasterList.Remove(el)
                Exit For
            End If
        Next


        evItem.DBRemoveMe()

    End Sub

    Public Sub RenameEvidencija(ByVal item As ScheduleAppointment, ByVal newSifra As String)
        Dim evItem As New ClsEvidItem

        evItem.EmployeeId = EmployeeId

        evItem.DBAction = -1
        evItem.UniqueId = item.UniqueID
        evItem.VrijemeOd = item.StartTime
        evItem.VrijemeDo = item.EndTime
        evItem.VrijemeUk = item.EndTime.Subtract(item.StartTime)
        evItem.Datum = item.StartTime.Date
        evItem.Sifra = item.Subject

        'For Each el As ClsEvidItem In EvEvidMasterList
        '    If el.UniqueId = evItem.UniqueId Then
        '        el.DBRenameMe(newSifra)
        '        Exit For
        '    End If
        'Next

        evItem.DBRenameMe(newSifra)

    End Sub



    Public Function InSfrList(ByVal __EvidItem_Subject As String) As Boolean
        Dim _InList As Boolean = False

        For Each el As ClsEvidItem In EvSfrLista
            If el.Sifra = __EvidItem_Subject Then
                _InList = True
                Exit For
            End If
        Next

        Return _InList

    End Function

    Public Sub InitListaSifri(ByVal pSifra As String, ByVal pExcluionList As String)

        Dim evEvidItem As ClsEvidItem
        evEvidItem = New ClsEvidItem

        evEvidItem.Sifra = pSifra
        evEvidItem.LabelColor = 9
        evEvidItem.ExclusionList = pExcluionList

        EvSfrLista.Add(evEvidItem)
        EvListaSifri.Add(pSifra, evEvidItem)


    End Sub

    Public Function GO_GetTotalsSD(ByVal __Godina As Integer) As ArrayList


        Dim _Suma As Integer = 0
        Dim _Sifra As String = ""
        Dim _Godina As String = ""
        Dim _Total As Integer = 0

        Dim itemTotal As ArrayList = New ArrayList()

        Return itemTotal


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            Dim dbRead As MySqlClient.MySqlDataReader
            Dim mycmd As MySqlCommand

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
SELECT COUNT(ep.`sifra_placanja`) AS sd, ep.`sifra_placanja`, YEAR(ep.`datum`) AS godina
                             FROM `evd_prisustva` ep WHERE ep.`employeeID`=?employeeID AND ep.`sifra_placanja` LIKE 'SD%'
                             AND  YEAR(ep.`datum`) = ?godina
                             GROUP BY YEAR(ep.`datum`), ep.`sifra_placanja`;
]]>.Value
            strSQL = strSQL.Replace("?employeeID", EmployeeId)
            strSQL = strSQL.Replace("?godina", __Godina)

            mycmd.CommandText = strSQL

            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@cnt", 0)

            mycmd.Prepare()

            Dim _Working As Integer = 0

            itemTotal.Add(New [ClsEvidItemTotal](1, "", "Stanje SD"))


            dbRead = mycmd.ExecuteReader
            Try
                Do While dbRead.Read
                    _Suma = ClsDatabase.DBGetInteger(dbRead("sd"))
                    _Sifra = ClsDatabase.DBGetText(dbRead("sifra_placanja"))
                    _Godina = ClsDatabase.DBGetInteger(dbRead("godina"))

                    If _Sifra = "SD-" Then
                        _Suma = -_Suma
                    End If
                    _Total += _Suma

                    itemTotal.Add(New [ClsEvidItemTotal](2, _Godina + "  -" + _Sifra, _Suma))
                Loop
            Catch ex As Exception
                ex.ToString()
            End Try
            dbRead.Close()

            itemTotal.Add(New [ClsEvidItemTotal](3, "Stanje:", _Total))
            itemTotal.Add(New [ClsEvidItemTotal](1, "", ""))

        End Using
    End Function

    Public Function GO_GetTotalsSP(ByVal __Godina As Integer) As ArrayList
        Dim dbRead As MySqlClient.MySqlDataReader
        Dim mycmd As MySqlCommand

        Dim _Suma As Integer = 0
        Dim _Sifra As String = ""
        Dim _Godina As String = ""
        Dim _Mjesec As String = ""
        Dim _Total As Integer = 0

        Dim itemTotal As ArrayList = New ArrayList()


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
SELECT COUNT(ep.`sifra_placanja`) AS sd, ep.`sifra_placanja`, YEAR(ep.`datum`) AS godina,
                             MONTH(ep.`datum`) AS mjesec
                             FROM `evd_prisustva` ep WHERE ep.`employeeID`=?employeeID AND ep.`sifra_placanja` LIKE 'SP%'
                             AND  YEAR(ep.`datum`) = ?godina
                             GROUP BY YEAR(ep.`datum`),MONTH(ep.`datum`), ep.`sifra_placanja`;
]]>.Value
            strSQL = strSQL.Replace("?employeeID", EmployeeId)
            strSQL = strSQL.Replace("?godina", __Godina)

            itemTotal.Add(New [ClsEvidItemTotal](1, "Stanje", "sl.put"))

            mycmd.CommandText = strSQL

            'mycmd.Parameters.Clear()


            Dim _Working As Integer = 0

            itemTotal.Add(New [ClsEvidItemTotal](1, "", "Stanje SD"))


            dbRead = mycmd.ExecuteReader
            Try
                Do While dbRead.Read
                    _Suma = ClsDatabase.DBGetInteger(dbRead("sd"))
                    _Sifra = ClsDatabase.DBGetText(dbRead("sifra_placanja"))
                    _Godina = ClsDatabase.DBGetInteger(dbRead("godina"))
                    _Mjesec = ClsDatabase.DBGetInteger(dbRead("mjesec"))

                    _Total += _Suma

                    itemTotal.Add(New [ClsEvidItemTotal](2, _Godina + "-" + _Mjesec, _Suma))
                Loop
            Catch ex As Exception
                ex.ToString()
            End Try
            dbRead.Close()


            strSQL = <![CDATA[ 
SELECT COUNT(ep.`sifra_placanja`) AS sd, ep.`sifra_placanja`, YEAR(ep.`datum`) AS godina
                             FROM `evd_prisustva` ep WHERE ep.`employeeID`=?employeeID AND ep.`sifra_placanja` LIKE 'SP%'
                             AND  YEAR(ep.`datum`) = ?godina
                             GROUP BY YEAR(ep.`datum`), ep.`sifra_placanja`;
]]>.Value
            strSQL = strSQL.Replace("?employeeID", EmployeeId)
            strSQL = strSQL.Replace("?godina", __Godina)

            itemTotal.Add(New [ClsEvidItemTotal](1, "Stanje", "sl.put"))

            mycmd.CommandText = strSQL

            'mycmd.Parameters.Clear()



            itemTotal.Add(New [ClsEvidItemTotal](1, "", "Stanje SD"))


            dbRead = mycmd.ExecuteReader
            Try
                Do While dbRead.Read
                    _Suma = ClsDatabase.DBGetInteger(dbRead("sd"))
                    _Sifra = ClsDatabase.DBGetText(dbRead("sifra_placanja"))
                    _Godina = ClsDatabase.DBGetInteger(dbRead("godina"))
                    _Mjesec = ""
                    _Total += _Suma
                Loop
            Catch ex As Exception
                ex.ToString()
            End Try
            dbRead.Close()

            itemTotal.Add(New [ClsEvidItemTotal](3, "Ukupno", "sl.put"))
            itemTotal.Add(New [ClsEvidItemTotal](4, _Godina + "-" + _Mjesec, _Suma))

        End Using

        Return itemTotal

    End Function

    Public Sub LoadDrzPraznici(ByVal pGodina As Integer)
        Dim dbRead As MySqlClient.MySqlDataReader
        Dim mycmd As MySqlCommand

        Dim txtOrgJed As String = ""
        Dim txtParOrgJed As String = ""
        Dim txtFootOrgJed As String = ""
        Dim txtSifra As String = ""

        cDrzPraznici.Clear()


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
SELECT sc.dt, sc.y, sc.m, sc.d FROM `sfr_calendar` sc WHERE YEAR(sc.dt)=?pGodina AND sc.`isHoliday`=1
]]>.Value
            strSQL = strSQL.Replace("?pGodina", pGodina.ToString)

            mycmd.CommandText = strSQL

            'mycmd.Parameters.Clear()


            dbRead = mycmd.ExecuteReader
            Try
                Do While dbRead.Read
                    Dim dt As Date = ClsDatabase.DBGetDate(dbRead("dt"))
                    cDrzPraznici.Add(dt)
                Loop
            Catch ex As Exception
                ex.ToString()
            End Try
            dbRead.Close()

        End Using
    End Sub

    Public Function GO_GetPrklBOL(ByVal pGodina As Integer) As Integer
        Dim dbRead As MySqlClient.MySqlDataReader
        Dim mycmd As MySqlCommand


        Dim retValue As Integer = 0


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
CALL EVD_preklap(@employeeID, @godina);
]]>.Value

            mycmd.CommandText = strSQL

            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@employeeID", EmployeeId)
            mycmd.Parameters.AddWithValue("@godina", pGodina)

            mycmd.Prepare()

            dbRead = mycmd.ExecuteReader
            Try
                Do While dbRead.Read
                    retValue = dbRead("BrDana")
                Loop
            Catch ex As Exception
                ex.ToString()
            End Try
            dbRead.Close()

        End Using

        Return retValue

    End Function

#Region "GO metode i funkcije"

    Public Sub FormGOPlacanja(ByVal __Godina As Integer)
        Dim _Sifra1 As String = "GO-" + (__Godina - 1).ToString
        Dim _Sifra2 As String = "GO-" + __Godina.ToString

        Dim evEvidItem As ClsEvidItem
        evEvidItem = New ClsEvidItem
        evEvidItem.Sifra = _Sifra1
        evEvidItem.LabelColor = 5
        evEvidItem.ExclusionList = _Sifra1 + "," + _Sifra2
        EvSfrLista.Add(evEvidItem)
        EvListaSifri.Add(_Sifra1, evEvidItem)


        evEvidItem = New ClsEvidItem
        evEvidItem.Sifra = _Sifra2
        evEvidItem.LabelColor = 7
        evEvidItem.ExclusionList = _Sifra1 + "," + _Sifra2
        EvSfrLista.Add(evEvidItem)
        EvListaSifri.Add(_Sifra2, evEvidItem)

    End Sub

    ''' <summary>
    ''' Čita parametre is sys_configuration.evidencije
    ''' </summary>
    ''' <remarks></remarks>
    Public Sub GO_GetInit()

        Dim dbRead As MySqlClient.MySqlDataReader
        Dim mycmd As MySqlCommand

        EvEvidMasterList.Clear()


        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection

            Dim strSQL As String = <![CDATA[ 
SELECT `adm_parameter`,`adm_value` FROM `sys_configuration` WHERE `adm_parameter` LIKE 'evidencije.%';
]]>.Value


            mycmd.CommandText = strSQL

            mycmd.Parameters.Clear()
            mycmd.Parameters.AddWithValue("@cnt", 0)

            mycmd.Prepare()




            dbRead = mycmd.ExecuteReader
            Try
                Do While dbRead.Read
                    Select Case ClsDatabase.DBGetText(dbRead("adm_parameter"))
                        Case "evidencije.goinit"
                            GO_InitGodina = ClsDatabase.DBGetInteger(dbRead("adm_value"))
                        Case "evidencije.allow_all"
                            GO_AllowAll = ClsDatabase.DBGetInteger(dbRead("adm_value"))

                        Case Else


                    End Select
                Loop
            Catch ex As Exception
                ex.ToString()
            End Try
            dbRead.Close()


        End Using
    End Sub




    'Public Function GO_GetTotals(ByVal __Godina As Integer) As ArrayList
    '    Dim sqlText As String
    '    Dim _IzrDani As Integer = 0
    '    Dim _IskDani As Integer = 0
    '    Dim _Sifra As String = "GO-" + __Godina.ToString


    '    sqlText = <sql text="SELECT ego.`IzrDani` + ego.`IzrDaniPreneseni`-ego.`IzrDaniAnul` AS IzrDani FROM `evd_godisnjiodmor` ego 
    '                        WHERE ego.`Godina`=?godina AND ego.EmployeeID=?employeeID;"/>.Attribute("text").Value
    '    sqlText = sqlText.Replace("?employeeID", EmployeeId)
    '    sqlText = sqlText.Replace("?godina", __Godina)

    '    Dim dbRead As MySqlClient.MySqlDataReader

    '    dbRead = ClsDatabase.CreateReader(sqlText)
    '    Do While dbRead.Read
    '        _IzrDani = ClsDatabase.DBGetInteger(dbRead("IzrDani"))
    '    Loop
    '    dbRead.Close()
    '    dbRead = Nothing

    '    sqlText = <sql text="SELECT COUNT(ep.`vrijeme_ukupno`) AS NumOfDays FROM `evd_prisustva` ep WHERE ep.`employeeID`=?employeeID AND 
    '            ep.`sifra_placanja`='?sifra' GROUP BY ep.`sifra_placanja`;"/>.Attribute("text").Value
    '    sqlText = sqlText.Replace("?employeeID", EmployeeId)
    '    sqlText = sqlText.Replace("?sifra", _Sifra)

    '    dbRead = ClsDatabase.CreateReader(sqlText)
    '    Do While dbRead.Read
    '        _IskDani = ClsDatabase.DBGetInteger(dbRead("NumOfDays"))
    '    Loop
    '    dbRead.Close()
    '    dbRead = Nothing

    '    Dim itemTotal As ArrayList = New ArrayList()

    '    itemTotal.Add(New [ClsEvidItemTotal](1, "", _Sifra))
    '    itemTotal.Add(New [ClsEvidItemTotal](2, "Po rješenju:", _IzrDani.ToString))
    '    itemTotal.Add(New [ClsEvidItemTotal](3, "Iskorišteno:", _IskDani.ToString))
    '    itemTotal.Add(New [ClsEvidItemTotal](4, "Preostalo:", (_IzrDani - _IskDani).ToString))
    '    itemTotal.Add(New [ClsEvidItemTotal](1, "", ""))

    '    GO_CurrYearPreostalo = _IzrDani - _IskDani
    '    GO_CurrYearIskoristeno = _IskDani

    '    __Godina = __Godina - 1
    '    _Sifra = "GO-" + __Godina.ToString
    '    _IzrDani = 0
    '    _IskDani = 0

    '    sqlText = <sql text="SELECT ego.`IzrDani` + ego.`IzrDaniPreneseni`-ego.`IzrDaniAnul` AS IzrDani FROM `evd_godisnjiodmor` ego 
    '                        WHERE ego.`Godina`=?godina AND ego.EmployeeID=?employeeID;"/>.Attribute("text").Value
    '    sqlText = sqlText.Replace("?employeeID", EmployeeId)
    '    sqlText = sqlText.Replace("?godina", __Godina)


    '    dbRead = ClsDatabase.CreateReader(sqlText)
    '    Do While dbRead.Read
    '        _IzrDani = ClsDatabase.DBGetInteger(dbRead("IzrDani"))
    '    Loop
    '    dbRead.Close()
    '    dbRead = Nothing

    '    sqlText = <sql text="SELECT COUNT(ep.`vrijeme_ukupno`) AS NumOfDays FROM `evd_prisustva` ep WHERE ep.`employeeID`=?employeeID AND 
    '            ep.`sifra_placanja`='?sifra' GROUP BY ep.`sifra_placanja`;"/>.Attribute("text").Value
    '    sqlText = sqlText.Replace("?employeeID", EmployeeId)
    '    sqlText = sqlText.Replace("?sifra", _Sifra)

    '    dbRead = ClsDatabase.CreateReader(sqlText)
    '    Do While dbRead.Read
    '        _IskDani = ClsDatabase.DBGetInteger(dbRead("NumOfDays"))
    '    Loop
    '    dbRead.Close()
    '    dbRead = Nothing


    '    itemTotal.Add(New [ClsEvidItemTotal](1, "", _Sifra))
    '    itemTotal.Add(New [ClsEvidItemTotal](2, "Po rješenju:", _IzrDani.ToString))
    '    itemTotal.Add(New [ClsEvidItemTotal](3, "Iskorišteno:", _IskDani.ToString))
    '    itemTotal.Add(New [ClsEvidItemTotal](4, "Preostalo:", (_IzrDani - _IskDani).ToString))
    '    itemTotal.Add(New [ClsEvidItemTotal](1, "", ""))

    '    GO_PrevYearPreostalo = _IzrDani - _IskDani

    '    Return itemTotal

    'End Function

#End Region

#Region "__EVS procedure"

    Public Sub EVS_Ins2Sched(ByVal pDatum As Date, ByVal pSifra As String, Optional ByVal pType As Integer = 1)

        Dim mycmd As MySqlCommand
        Dim strSQL As String

        strSQL = <![CDATA[ 
CALL EVS_prisustvo_sched (?employeeID, '?sifra_placanja', ?datum, ?type);
]]>.Value

        strSQL = <sql text=""/>.Attribute("text").Value
        strSQL = strSQL.Replace("?employeeID", EmployeeId)
        strSQL = strSQL.Replace("?sifra_placanja", pSifra)
        strSQL = strSQL.Replace("?datum", ClsDatabase.DBDate(pDatum))
        strSQL = strSQL.Replace("?type", pType)

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection


            mycmd.CommandText = strSQL

            'mycmd.Parameters.Clear()


            mycmd.ExecuteNonQuery()

        End Using


    End Sub

#End Region

End Class

#Region "Evidencija Item"


Public Class ClsEvidItem

    Public EmployeeId As Integer
    Public UniqueId As Integer

    Public Sifra As String
    Public Sifra2 As String
    Public LabelColor As Integer
    Public ExclusionList As String

    Public Datum As Date
    Public VrijemeOd As Date
    Public VrijemeDo As Date
    Public VrijemeUk As TimeSpan

    Public Limit As Integer
    Public Iznos As Double

    Public DBAction As Integer = 0  ' -1 brisanje, +1 insert ili update

    Public Sub Clear()
        Sifra = ""
        Sifra2 = ""
        LabelColor = 0
        ExclusionList = ""
        Limit = 0
        Iznos = 0
    End Sub

    Public Sub DBAddMe()

        Dim mycmd As MySqlCommand
        Dim strSQL As String

        strSQL = <![CDATA[ 
INSERT INTO `evd_prisustva` (`employeeID`, `sifra_placanja`, `datum`, `vrijeme_od`, `vrijeme_do`, `vrijeme_ukupno`)
VALUES( ?employeeID, '?sifra_placanja', ?datum, ?vrijeme_od, ?vrijeme_do, ?vrijeme_ukupno);
]]>.Value

        strSQL = strSQL.Replace("?employeeID", EmployeeId)
        strSQL = strSQL.Replace("?sifra_placanja", Sifra)
        strSQL = strSQL.Replace("?datum", ClsDatabase.DBDate(Datum))
        strSQL = strSQL.Replace("?vrijeme_od", ClsDatabase.DBTime(VrijemeOd))
        strSQL = strSQL.Replace("?vrijeme_do", ClsDatabase.DBTime(VrijemeDo))
        strSQL = strSQL.Replace("?vrijeme_ukupno", ClsDatabase.DBTime(VrijemeUk))

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection


            mycmd.CommandText = strSQL

            'mycmd.Parameters.Clear()


            mycmd.ExecuteNonQuery()


            strSQL = <![CDATA[ "SELECT LAST_INSERT_ID();"]]>.Value

            Dim dbRead As MySqlClient.MySqlDataReader

            dbRead = mycmd.ExecuteReader
            Do While dbRead.Read
                UniqueId = dbRead("LAST_INSERT_ID()")
            Loop

        End Using
    End Sub

    Public Sub DBRemoveMe()

        Dim mycmd As MySqlCommand
        Dim strSQL As String

        strSQL = <![CDATA[ 
DELETE FROM `evd_prisustva` WHERE `evd_prisustva_id` = ?evd_prisustva_id;
]]>.Value

        strSQL = strSQL.Replace("?evd_prisustva_id", UniqueId)

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection


            mycmd.CommandText = strSQL

            'mycmd.Parameters.Clear()


            mycmd.ExecuteNonQuery()

        End Using

    End Sub

    Public Sub DBRenameMe(ByVal newSifra As String)
        Dim mycmd As MySqlCommand
        Dim strSQL As String

        strSQL = <![CDATA[ 
UPDATE `evd_prisustva` SET sifra_placanja='?sifra_placanja'  WHERE `evd_prisustva_id` = ?evd_prisustva_id;
]]>.Value

        strSQL = strSQL.Replace("?evd_prisustva_id", UniqueId)
        strSQL = strSQL.Replace("?sifra_placanja", newSifra)

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection


            mycmd.CommandText = strSQL

            mycmd.ExecuteNonQuery()

        End Using
    End Sub

    Public Sub DBUpdateMe()
        Dim mycmd As MySqlCommand
        Dim strSQL As String

        strSQL = <![CDATA[ 
INSERT INTO `evd_prisustva` (`employeeID`, `sifra_placanja`, `datum`, `vrijeme_od`, `vrijeme_do`, `vrijeme_ukupno`)
VALUES( ?employeeID, ?sifra_placanja, ?datum, ?vrijeme_od, ?vrijeme_do, ?vrijeme_ukupno);
]]>.Value

        Using myconnection As New MySqlClient.MySqlConnection(ConnectionString)
            myconnection.Open()

            mycmd = New MySqlCommand
            mycmd.Connection = myconnection


            mycmd.CommandText = strSQL

            mycmd.ExecuteNonQuery()

        End Using
    End Sub
End Class

Public Class ClsEvidItemTotal
    'Public EmployeeId As Integer
    Public m_Id As Integer
    Private m_Sifra As String
    Public m_Sifra2 As String

    Public Sub New(ByVal _Id As Integer, ByVal _Sifra As String, ByVal _Sifra2 As String)
        m_Id = _Id
        m_Sifra = _Sifra
        m_Sifra2 = _Sifra2
    End Sub

    Public Property Sifra() As String
        Get
            Return m_Sifra
        End Get
        Set(ByVal value As String)
            m_Sifra = value
        End Set
    End Property

    Public Property Sifra2() As String
        Get
            Return m_Sifra2
        End Get
        Set(ByVal value As String)
            m_Sifra2 = value
        End Set
    End Property
End Class
#End Region
