Imports Microsoft.VisualBasic

Public Class NgPnPutniNalogDet

    Public Property id As Integer
    Public Property pn_id As Integer
    Public Property pn_oblast As Integer
    Public Property relacija As String
    Public Property dat_polaska As Date
    Public Property dat_povratka As Date
    Public Property broj_sati As Decimal
    Public Property broj_dnevnica As Decimal
    Public Property iznos_dnevnice As Decimal
    Public Property ukupan_iznos As Decimal
    Public Property vrsta_prevoza As String
    Public Property razred As String
    Public Property iznos_karte As Decimal
    Public Property naknada_km As Decimal
    Public Property troskovi_goriva As Decimal
    Public Property troskovi_parkinga As Decimal
    Public Property ostali_troskovi As Decimal
    Public Property obr_ostalih_troskova As String
    Public Property order_id As Integer
    Public Property Done As Boolean

End Class
