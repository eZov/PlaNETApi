
#Region "Copyright Syncfusion Inc. 2001-2016."
' Copyright Syncfusion Inc. 2001-2016. All rights reserved.
' Use of this code is subject to the terms of our license.
' A copy of the current license can be obtained at any time by e-mailing
' licensing@syncfusion.com. Any infringement will be prosecuted under
' applicable laws. 
#End Region
Imports System.Globalization
Imports System.Drawing
Imports System.Collections
Imports System.ComponentModel
Imports System.Data
Imports System.IO
Imports System.Runtime.Serialization
Imports System.Runtime.Serialization.Formatters
Imports System.Runtime.Serialization.Formatters.Binary
Imports Syncfusion.Schedule
Imports Syncfusion.Windows.Forms.Schedule




#Region "DataProvider"
''' <summary>
''' Derives <see cref="ScheduleDataProvider"/> to implement <see cref="IScheduleDataProvider"/>.
''' </summary>
''' <remarks>
''' This implementation of IScheduleDataProvider uses a collection of <see cref="SimpleScheduleAppointment"/>
''' objects to hold the items displayed in the schedule. This collection is serialized to disk as a 
''' binary file. The serialization is restricted to the SimpleScheduleDataProvider.MasterList and the
''' SimpleScheduleAppointment objects that it holds. 
''' </remarks>
Public Class SimpleScheduleDataProvider
    Inherits ScheduleDataProvider
    ''' <summary>
    ''' Default constructor.
    ''' </summary>
    Public Sub New()
        MyBase.New()
    End Sub

    Private m_fileName As String

    Public Property FileName() As String
        Get
            Return m_fileName
        End Get
        Set(ByVal value As String)
            m_fileName = value
        End Set
    End Property

    Private m_masterList As SimpleScheduleAppointmentList

    ''' <summary>
    ''' Get or sets an IScheduleAppointmentList collection that holds the IScheduleAppointments. 
    ''' </summary>
    Public Property MasterList() As SimpleScheduleAppointmentList
        Get
            Return m_masterList
        End Get
        Set(ByVal value As SimpleScheduleAppointmentList)
            m_masterList = value
        End Set
    End Property

#Region "random data"

    ''' <summary>
    ''' A static method that provides random data, not really a part of the implementations.
    ''' </summary>
    ''' <returns>A SimpleScheduleAppointmentList object holding sample data.</returns>
    Public Shared Function InitializeRandomData() As SimpleScheduleAppointmentList
        'int tc = Environment.TickCount;
        'int tc = 26260100;// simple spread 
        Dim tc As Integer = 28882701
        ' split the appointment across midnight & 3 items at 8am on 2 days ago
        'Console.WriteLine("Random seed: {0}", tc);
        Dim r As New Random(tc)
        Dim r1 As New Random(tc)

        ' set the number of sample items you want in this list.
        'int count = r.Next(20) + 4;
        Dim count As Integer = 20
        '1000;//200;//30;
        Dim masterList As New SimpleScheduleAppointmentList()
        Dim now As DateTime = DateTime.Now.[Date]

        For i As Integer = 0 To count - 1
            Dim item As ScheduleAppointment = TryCast(masterList.NewScheduleAppointment(), ScheduleAppointment)

            Dim dayOffSet As Integer = 30 - r.[Next](60)

            Dim hourOffSet As Integer = 24 - r.[Next](48)

            Dim len As Integer = 30 * (r.[Next](4) + 1)
            item.StartTime = now.AddDays(CDbl(dayOffSet)).AddHours(CDbl(hourOffSet))


            item.EndTime = item.StartTime.AddMinutes(CDbl(len))
            item.Subject = String.Format("subject{0}", i)
            item.Content = String.Format("content{0}", i)
            item.LabelValue = If(r1.[Next](10) < 3, 0, r1.[Next](10))
            item.LocationValue = String.Format("location{0}", r1.[Next](5))

            'item.ReminderValue = If(r1.[Next](10) < 5, 0, r1.[Next](12))
            'item.Reminder = r1.[Next](10) > 1
            'item.AllDay = r1.[Next](10) < 1


            item.MarkerValue = r1.[Next](4)
            item.Dirty = False
            masterList.Add(item)
        Next

        '/set explicit values if needed for testing...
        'masterList[142].Reminder = true;
        'masterList[142].ReminderValue = 9;//  hrs; // 7;//3 hrs


        'DisplayList("Before Sort", masterList);
        masterList.SortStartTime()
        'DisplayList("After Sort", masterList);

        Return masterList
    End Function

    Public Shared Function InitializeRandomDataSource() As SimpleScheduleAppointmentList
        'int tc = Environment.TickCount;
        'int tc = 26260100;// simple spread 
        Dim tc As Integer = 20000000
        ' split the appointment across midnight & 3 items at 8am on 2 days ago
        'Console.WriteLine("Random seed: {0}", tc);
        Dim r As New Random(tc)
        Dim r1 As New Random(tc)

        ' set the number of sample items you want in this list.
        'int count = r.Next(20) + 4;
        Dim count As Integer = 200
        '1000;//200;//30;
        Dim masterList As New SimpleScheduleAppointmentList()
        Dim now As DateTime = DateTime.Now.[Date]

        For i As Integer = 0 To count - 1
            Dim item As ScheduleAppointment = TryCast(masterList.NewScheduleAppointment(), ScheduleAppointment)

            Dim dayOffSet As Integer = 30 - r.[Next](60)

            Dim hourOffSet As Integer = 24 - r.[Next](48)

            Dim len As Integer = 30 * (r.[Next](4) + 1)
            item.StartTime = now.AddDays(CDbl(dayOffSet)).AddHours(CDbl(hourOffSet))


            item.EndTime = item.StartTime.AddMinutes(CDbl(len))
            item.Subject = String.Format("subject{0}", i)
            item.Content = String.Format("content{0}", i)
            item.LabelValue = If(r1.[Next](10) < 3, 0, r1.[Next](10))
            item.LocationValue = String.Format("location{0}", r1.[Next](5))

            item.ReminderValue = If(r1.[Next](10) < 5, 0, r1.[Next](12))
            item.Reminder = r1.[Next](10) > 1
            item.AllDay = r1.[Next](10) < 1


            item.MarkerValue = r1.[Next](4)
            item.Dirty = False
            masterList.Add(item)
        Next

        '/set explicit values if needed for testing...
        'masterList[142].Reminder = true;
        'masterList[142].ReminderValue = 9;//  hrs; // 7;//3 hrs


        'DisplayList("Before Sort", masterList);
        masterList.SortStartTime()
        'DisplayList("After Sort", masterList);

        Return masterList
    End Function
    Private Shared Sub DisplayList(ByVal title As String, ByVal list As ScheduleAppointmentList)
#If console Then
			Console.WriteLine(Convert.ToString("*************") & title)
			For Each item As ScheduleAppointment In list
				Console.WriteLine(item)
			Next
#End If
    End Sub
#End Region

#Region "Evidencije"
    Public Sub InitializeMasterList()

        m_masterList = New SimpleScheduleAppointmentList()

    End Sub

    Public Sub AddEvidencija(ByVal evDateTime As DateTime, _
                                    ByVal evVrsta As String, ByVal evId As Integer, ByVal evLblCol As Integer, ByVal evExcl As String, _
                                    ByRef evEvid As ClsGOEvidencije)

        'Dim now As DateTime = DateTime.Now.[Date]


        Dim item As ScheduleAppointment = TryCast(masterList.NewScheduleAppointment(), ScheduleAppointment)

        item.StartTime = evDateTime.AddHours(8)
        item.EndTime = item.StartTime.AddHours(8)

        item.Subject = evVrsta
        item.Content = evVrsta
        item.LabelValue = evLblCol
        item.LocationValue = ""

        item.UniqueID = evId

        item.MarkerValue = 2
        item.Dirty = False

        Dim itemFound As ScheduleAppointment = Nothing
        For Each el As ScheduleAppointment In masterList
            If el.StartTime.Date = evDateTime.Date And (el.Subject = evVrsta Or evExcl.Contains(el.Subject)) Then
                itemFound = el
            End If
        Next

        'Dim itemFound As ScheduleAppointment = masterList.Find(evId)

        If itemFound IsNot Nothing Or evDateTime.DayOfWeek = DayOfWeek.Saturday Or evDateTime.DayOfWeek = DayOfWeek.Sunday Then
            Exit Sub
        End If

        evEvid.AddEvidencija(item)
        masterList.Add(item)

    End Sub

    Public Sub AddEvidencija(ByVal evDateTime As DateTime, _
                                ByVal evVrsta As String, ByVal evId As Integer, ByVal evLblCol As Integer, _
                                ByRef evEvid As ClsGOEvidencije)

        'Dim now As DateTime = DateTime.Now.[Date]

        If Not AllowedDate(evDateTime, evVrsta, evEvid) Then
            Exit Sub
        End If

        Dim item As ScheduleAppointment = TryCast(MasterList.NewScheduleAppointment(), ScheduleAppointment)

        item.StartTime = evDateTime.AddHours(8)
        item.EndTime = item.StartTime.AddHours(8)

        item.Subject = evVrsta
        item.Content = evVrsta
        item.LabelValue = evLblCol
        item.LocationValue = ""

        item.UniqueID = evId

        item.MarkerValue = 2
        item.Dirty = False

        Dim itemFound As ScheduleAppointment = Nothing
        For Each el As ScheduleAppointment In MasterList
            If el.StartTime.Date = item.StartTime.Date AndAlso el.Subject = item.Subject _
            AndAlso el.StartTime = item.StartTime AndAlso el.EndTime = item.EndTime Then
                itemFound = el
                Exit For
            End If
        Next

        'For Each el As ScheduleAppointment In masterList
        '    If evEvid.ExclusionList(evVrsta).Contains(el.Subject) Then
        '        itemFound = el
        '    End If
        'Next

        If itemFound IsNot Nothing Then
            Exit Sub
        End If

        evEvid.AddEvidencija(item)
        MasterList.Add(item)

        If evVrsta = "RRN" OrElse evVrsta = "RRP" Then
            evEvid.EVS_Ins2Sched(evDateTime, evVrsta)
        End If

        If evEvid.IsPraznik(evDateTime) And evVrsta = "SP" Then
            evEvid.EVS_Ins2Sched(evDateTime, evVrsta)
        ElseIf evDateTime.DayOfWeek = DayOfWeek.Sunday And evVrsta = "SP" Then
            evEvid.EVS_Ins2Sched(evDateTime, evVrsta)
        End If

    End Sub

    Public Sub RemoveEvidencija(ByVal evDateTime As DateTime, ByVal evId As Integer, ByVal evVrsta As String, ByRef evEvid As ClsGOEvidencije)

        For Each el As ScheduleAppointment In MasterList
            If el.UniqueID = evId And el.StartTime.Date = evDateTime.Date And el.Subject = evVrsta Then

                evEvid.RemoveEvidencija(el)
                If evVrsta = "RRN" OrElse evVrsta = "RRP" Then
                    evEvid.EVS_Ins2Sched(evDateTime, evVrsta, -1)
                End If

                MasterList.Remove(el)

                Exit Sub
            End If
        Next

    End Sub

    Public Sub RenameEvidencija(ByVal evDateTime As DateTime, ByVal evId As Integer, ByVal evVrstaFrom As String, ByVal evVrstaTo As String, ByRef evEvid As ClsGOEvidencije)

        For Each el As ScheduleAppointment In MasterList
            If el.UniqueID = evId And el.StartTime.Date = evDateTime.Date And el.Subject = evVrstaFrom Then
                el.Subject = evVrstaTo
                evEvid.RenameEvidencija(el, evVrstaTo)
                Exit Sub

            End If
        Next

    End Sub

#End Region

#Region "__Provjere (RRN i RRP)"

    Public Function AllowedDate(ByVal evDateTime As DateTime, ByVal evVrsta As String, ByRef evEvid As ClsGOEvidencije) As Boolean
        Dim _retVal As Boolean = False

        Select Case evVrsta
            Case "RRN"
                If evDateTime.DayOfWeek = DayOfWeek.Sunday Then
                    _retVal = True
                End If

            Case "RRP"
                For Each el As Date In evEvid.DrzPraznici
                    If evDateTime = el Then
                        _retVal = True
                        Exit For
                    End If
                Next

            Case Else
                _retVal = True
        End Select


        Return _retVal

    End Function


#End Region

#Region "base class overrides"

    ''' <summary>
    ''' Returns a the subset of MasterList between the 2 dates.
    ''' </summary>
    ''' <param name="startDate">Starting date limit for the returned items.</param>
    ''' <param name="endDate">Ending date limit for the returned items.</param>
    ''' <returns>Returns a the subset of MasterList.</returns>
    Public Overrides Function GetSchedule(ByVal startDate As DateTime, ByVal endDate As DateTime) As IScheduleAppointmentList
        Dim list As New ScheduleAppointmentList()
        Dim start As DateTime = startDate.[Date]
        Dim [end] As DateTime = endDate.[Date]
        For Each item As ScheduleAppointment In Me.MasterList
            'item.EndTime.AddMinutes(-1) is to make sure an item that ends at 
            'midnight is not shown on the next days calendar

            If (item.StartTime.[Date] >= start AndAlso item.StartTime.[Date] <= [end]) OrElse (item.EndTime.AddMinutes(-1).[Date] > start AndAlso item.EndTime.[Date] <= [end]) Then
                list.Add(item)
            End If
        Next
        list.SortStartTime()
        'DisplayList(string.Format("************dates between {0} and {1}", startDate, endDate), list);
        Return list
    End Function

    ''' <summary>
    ''' Returns a the subset of MasterList between the 2 dates.
    ''' </summary>
    ''' <param name="day">Date for the returned items.</param>
    ''' <returns>Returns a the subset of MasterList.</returns>
    Public Overrides Function GetScheduleForDay(ByVal day As DateTime) As IScheduleAppointmentList
        Dim list As New ScheduleAppointmentList()
        day = day.[Date]
        For Each item As ScheduleAppointment In Me.MasterList
            'do not want anything that ends at 12AM on the day
            If item.StartTime.[Date] = day OrElse (item.EndTime.[Date] = day AndAlso item.EndTime > day) Then
                list.Add(item)
            End If
        Next

        'DisplayList(string.Format("*************day {0}", day), list);
        Return list
    End Function

    ''' <summary>
    ''' Saves the MasterList as a diskfile.
    ''' </summary>
    Public Overrides Sub CommitChanges()
        SaveBinary(FileName)
        Me.IsDirty = False
    End Sub

    ''' <summary>
    ''' Gets or sets whether the MasterList has been modified.
    ''' </summary>
    Public Overrides Property IsDirty() As Boolean
        Get
            Dim val As Boolean = MyBase.IsDirty
            If Not val Then
                'if no global setting marked list as dirty, check individual items
                For Each item As IScheduleAppointment In Me.MasterList
                    If item.Dirty Then
                        val = True
                        Exit For
                    End If
                Next
            End If
            Return val
        End Get
        Set(ByVal value As Boolean)
            MyBase.IsDirty = value
        End Set
    End Property


    ''' <summary>
    ''' Saves the current <see cref="MasterList"/> object in binary format to a file 
    ''' with the specified filename.
    ''' </summary>
    Public Sub SaveBinary(ByVal fileName As String)
        Dim s As Stream = File.Create(fileName)
        SaveBinary(s)
        s.Close()
    End Sub

    ''' <summary>
    ''' Saves the current <see cref="MasterList"/> object to a stream in binary format.
    ''' </summary>
    Public Sub SaveBinary(ByVal s As Stream)
        Dim b As New BinaryFormatter()
        b.AssemblyFormat = FormatterAssemblyStyle.Simple
        b.Serialize(s, Me.MasterList)
    End Sub


    ''' <summary>
    ''' Creates an instance of <see cref="SimpleScheduleDataProvider"/> and loads 
    ''' a previously serialized MasterList into the instance.
    ''' </summary>
    ''' <param name="fileName">The serialized filename.</param>
    ''' <returns>A SimpleScheduleDataProvider.</returns>
    ''' <remarks>
    ''' This method uses see cref=AppDomain.CurrentDomain.AssemblyResolve/ to 
    ''' avoid versioning issues with the binary serialization of the MasterList.
    ''' </remarks>
    Public Shared Function LoadBinary(ByVal fileName As String) As SimpleScheduleDataProvider
        Dim t As New SimpleScheduleDataProvider()
        Dim s As Stream = File.OpenRead(fileName)
        Try
            AddHandler AppDomain.CurrentDomain.AssemblyResolve, AddressOf Syncfusion.ScheduleWindowsAssembly.AssemblyResolver
            Dim b As New BinaryFormatter()
            b.AssemblyFormat = FormatterAssemblyStyle.Simple
            Dim obj As Object = b.Deserialize(s)

            t.MasterList = TryCast(obj, SimpleScheduleAppointmentList)
        Finally
            s.Close()
            RemoveHandler AppDomain.CurrentDomain.AssemblyResolve, AddressOf Syncfusion.ScheduleWindowsAssembly.AssemblyResolver
        End Try
        Return t
    End Function

    ''' <summary>
    ''' Overridden to return a <see cref="SimpleScheduleAppointment"/>.
    ''' </summary>
    ''' <returns></returns>
    Public Overrides Function NewScheduleAppointment() As IScheduleAppointment
        Return New SimpleScheduleAppointment()
    End Function

    ''' <summary>
    ''' Overridden to add the item to the MasterList.
    ''' </summary>
    ''' <param name="item">IScheduleAppointment item to be added.</param>
    Public Overrides Sub AddItem(ByVal item As IScheduleAppointment)
        Me.MasterList.Add(item)
    End Sub

    ''' <summary>
    ''' Overridden to remove the item from the MasterList.
    ''' </summary>
    ''' <param name="item">IScheduleAppointment item to be removed.</param>
    Public Overrides Sub RemoveItem(ByVal item As IScheduleAppointment)
        Me.MasterList.Remove(item)
    End Sub
#End Region
End Class
#End Region

#Region "ScheduleAppointmentList"

''' <summary>
''' Derives <see cref="ScheduleAppointmentList"/> to implement IScheduleAppointmentList.
''' </summary>
<Serializable()> _
Public Class SimpleScheduleAppointmentList
    Inherits ScheduleAppointmentList
    Implements ISerializable
#Region "ISerializable Members"

#Region "ISerializable Members"

    ''' <summary>
    ''' Used in serialization.
    ''' </summary>
    ''' <param name="info"> The SerializationInfo.</param>
    ''' <param name="context">The StreamingContext.</param>
    Private Sub ISerializable_GetObjectData(ByVal info As SerializationInfo, ByVal context As StreamingContext) Implements ISerializable.GetObjectData
        GetObjectData(info, context)
    End Sub

#End Region


    ''' <summary>
    ''' Override to control serialization.
    ''' </summary>
    ''' <param name="info"> The SerializationInfo.</param>
    ''' <param name="context">The StreamingContext.</param>
    Protected Overridable Sub GetObjectData(ByVal info As SerializationInfo, ByVal context As StreamingContext)
        info.AddValue("List", Me.List)
    End Sub


#End Region

    ''' <summary>
    ''' Used in serialization.
    ''' </summary>
    ''' <param name="info"> The SerializationInfo.</param>
    ''' <param name="context">The StreamingContext.</param>
    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)
        Me.List = DirectCast(info.GetValue("List", GetType(ArrayList)), ArrayList)
    End Sub

    ''' <summary>
    ''' Default constructor.
    ''' </summary>
    Public Sub New()

        MyBase.New()
    End Sub

    ''' <summary>
    ''' Overridden to return a <see cref="SimpleScheduleAppointment"/>.
    ''' </summary>
    ''' <returns>A SimpleScheduleAppointment.</returns>
    Public Overrides Function NewScheduleAppointment() As IScheduleAppointment
        Return New SimpleScheduleAppointment()
    End Function


End Class
#End Region

#Region "ScheduleAppointment"

''' <summary>
''' Derives <see cref="ScheduleAppointment"/> to implement IScheduleAppointment.
''' </summary>
<Serializable()> _
Public Class SimpleScheduleAppointment
    Inherits ScheduleAppointment
    Implements ISerializable
#Region "ISerializable Members"

    ''' <summary>
    ''' Default constructor.
    ''' </summary>
    Public Sub New()
        MyBase.New()
    End Sub

    ''' <summary>
    ''' Overridden to handle serilaization.
    ''' </summary>
    ''' <param name="info">The SerialazationInfo.</param>
    ''' <param name="context">The StreamingContext.</param>
    Protected Sub New(ByVal info As SerializationInfo, ByVal context As StreamingContext)
        Me.UniqueID = CInt(info.GetValue("UniqueID", GetType(Integer)))
        Me.Subject = DirectCast(info.GetValue("Subject", GetType(String)), String)
        Me.StartTime = DirectCast(info.GetValue("StartTime", GetType(DateTime)), DateTime)
        Me.ReminderValue = CInt(info.GetValue("ReminderValue", GetType(Integer)))
        Me.Reminder = CBool(info.GetValue("Reminder", GetType(Boolean)))
        Me.Owner = CInt(info.GetValue("Owner", GetType(Integer)))
        Me.MarkerValue = CInt(info.GetValue("MarkerValue", GetType(Integer)))
        Me.LocationValue = DirectCast(info.GetValue("LocationValue", GetType(String)), String)
        Me.LabelValue = CInt(info.GetValue("LabelValue", GetType(Integer)))
        Me.EndTime = DirectCast(info.GetValue("EndTime", GetType(DateTime)), DateTime)
        Me.Content = DirectCast(info.GetValue("Content", GetType(String)), String)
        Me.AllDay = CBool(info.GetValue("AllDay", GetType(Boolean)))

        Me.Dirty = False
    End Sub

    ''' <summary>
    ''' Handle serilaization.
    ''' </summary>
    ''' <param name="info">The SerialazationInfo.</param>
    ''' <param name="context">The StreamingContext.</param>
    Public Sub GetObjectData(ByVal info As SerializationInfo, ByVal context As StreamingContext) Implements ISerializable.GetObjectData
        info.AddValue("UniqueID", Me.UniqueID)
        info.AddValue("Subject", Me.Subject)
        info.AddValue("StartTime", Me.StartTime)
        info.AddValue("ReminderValue", Me.ReminderValue)
        info.AddValue("Reminder", Me.Reminder)
        info.AddValue("Owner", Me.Owner)
        info.AddValue("MarkerValue", Me.MarkerValue)
        info.AddValue("LocationValue", Me.LocationValue)
        info.AddValue("LabelValue", Me.LabelValue)
        info.AddValue("EndTime", Me.EndTime)
        info.AddValue("Content", Me.Content)
        info.AddValue("AllDay", Me.AllDay)

        'info.AddValue("Tag", this.Tag); assume Tag not serializable in this implemetation
    End Sub

#End Region

End Class
#End Region



