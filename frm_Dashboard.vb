'Room Scheduling System
'This Version was ported from MySQL to SQLite
'By: Hanz Aquino
'
'DB tables   : dbscheme.(rooms, room_scheduls, room_account)
'
'03-25-21    : Designed the UI concept in Photoshop
'03-26-21    : Started the UI/UX design in Visual Studio 2008
'03-27-21    : Main Dashboard layout done
'            : started database functionality before proceeding to other layouts
'            : Added the algorithm for finding vacant rooms
'            : 'Add schedule' layout in UI/UX
'            : Fixed some bugs in Add Schedule Panel
'            : Finished the add Schedule UI and Functionality
'03-28-21    : 'Add Room' layout in UI/UX
'            : Added Functions in mod_DBConnection including removal of finshed schedules from the DB
'            : Fixed the issue where you can enter a date that aldready passed
'            : Converted the FROM and TO columns in room_schedlues table into BIGINT in order to function not as an String
'            : MAIN ISSUE: Program is loading slow when communicating to the DB
'03-29-21    : Added loading screen
'            : Added logo and icons
'03-30-21    : Created datase table for Accounts
'            : Added account functions
'            : Created the UI/UX for LOGIN
'            : Added USERNAME-PASSWORD Authentication
'            : Passwords are encrypted as Base64 on top of MD5 encryption
'03-31-21    : Added Create Account UI/UX
'            : Added few lines oc code to ensure that Account don't duplicate
'			 : 609 lines of Code (UI Code not included)
'			 : 1862 lines of Code (UI code included)
'			 : Functionality to be added: ADD/REMOVE ROOMS AND SCHEDULES
'			 : Functionality to be added: Logged in user name in UI
'04-1-21     : Added the ability to Remove schedules
'            : :::: Created a version that runs with SQLite instead of MySQL (for faster database connectivity)




Public Class frm_Dashboard
    Dim dateNow As Date = Date.Now()
    Dim current_USER As String = ""

    'INITIALIZATIONS

    Private Sub Form1_Load(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles MyBase.Load

        frm_loadingScreen.Show()
        frm_loadingScreen.ProgressBar_Dashboard.Value = 0
        frm_loadingScreen.Loading_lbl.Text = "Initializing Database Connection..."
        Application.DoEvents()
        init_connection()
        frm_loadingScreen.ProgressBar_Dashboard.Value = 10
        'Initialize available rooms for today from 8-12
        frm_loadingScreen.Loading_lbl.Text = "Initializing Panels..."
        Application.DoEvents()
        initRightSidePanel()
        frm_loadingScreen.ProgressBar_Dashboard.Value = 50
        frm_loadingScreen.Loading_lbl.Text = "Initializing Rooms..."
        Application.DoEvents()
        initRoomsListBox()
        frm_loadingScreen.ProgressBar_Dashboard.Value = 80
        frm_loadingScreen.Loading_lbl.Text = "Initializing Schedules..."
        Application.DoEvents()
        removeFinishedSchedules(dateNow)
        frm_loadingScreen.ProgressBar_Dashboard.Value = 100
        frm_loadingScreen.Close()

        If current_USER = "" Then
            Me.Text = "Room Sched - Login"
            Panel1.Visible = False
            Panel_LOGIN.Visible = True
            current_USER = ""
        End If



        Me.FormBorderStyle = Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False



        '''''TEST''''''
        '' 7 - 10 test
        'addSchedule("Room-H-1", New Date(2021, 3, 29, 1, 0, 0).Ticks.ToString, New Date(2021, 3, 29, 3, 0, 0).Ticks.ToString)
        'addSchedule("Room-H-1", New Date(2021, 3, 29, 4, 0, 0).Ticks.ToString, New Date(2021, 3, 29, 7, 0, 0).Ticks.ToString)
        'addSchedule("Room-H-1", New Date(2021, 3, 29, 7, 0, 0).Ticks.ToString, New Date(2021, 3, 29, 10, 0, 0).Ticks.ToString)
        'addSchedule("Room-H-2", New Date(2021, 3, 29, 1, 0, 0).Ticks.ToString, New Date(2021, 3, 29, 3, 0, 0).Ticks.ToString)
        'addSchedule("Room-H-3", New Date(2021, 3, 29, 4, 0, 0).Ticks.ToString, New Date(2021, 3, 29, 6, 0, 0).Ticks.ToString)
        'addSchedule("Room-H-4", New Date(2021, 3, 29, 1, 0, 0).Ticks.ToString, New Date(2021, 3, 29, 5, 0, 0).Ticks.ToString)
        'addSchedule("Room-H-5", New Date(2021, 3, 29, 16, 0, 0).Ticks.ToString, New Date(2021, 3, 29, 21, 0, 0).Ticks.ToString)
        'addSchedule("Room-H-6", New Date(2021, 3, 29, 21, 0, 0).Ticks.ToString, New Date(2021, 3, 29, 22, 0, 0).Ticks.ToString)
        'For Each room In findAvailable(New Date(2021, 3, 29, 7, 0, 0).Ticks, New Date(2021, 3, 29, 10, 0, 0).Ticks)
        '    Console.WriteLine(room)
        'Next
        '''''''''''''''
    End Sub

    Public Sub initRightSidePanel()
        lbl_dateNow.Text = Format(Now, "dddd MMMM dd, yyyy")
        list_avlrooms.Items.Clear()
        For Each room In findAvailable(New Date(dateNow.Year, dateNow.Month, dateNow.Day, dateNow.Hour, 0, 0).Ticks, New Date(dateNow.Year, dateNow.Month, dateNow.Day, 23, 59, 59).Ticks)
            list_avlrooms.Items.Add(room)
        Next
        lbl_avl_roomcount.Text = Str(list_avlrooms.Items.Count) & " rooms available..."
    End Sub

    Public Sub initRoomsListBox()
        DataGridView_ROOMS.DataSource = getRoomsSchedTable()
        DataGridView_ROOMS.Columns.Remove("FROM")
        DataGridView_ROOMS.Columns.Remove("TO")
    End Sub

    Public Sub logInAuthorized()
        Me.Text = "Room Sched - Menu"
        resetUser()
        Me.FormBorderStyle = Windows.Forms.FormBorderStyle.Sizable
        Me.MaximizeBox = True
        Panel_LOGIN.Visible = False
        Panel1.Visible = True
        txt_USERNAME.Clear()
        txt_PASSWORD.Clear()
        Me.Refresh()
    End Sub

    Public Sub logOut()
        Me.Text = "Room Sched - Login"
        Panel1.Visible = False
        Panel_LOGIN.Visible = True
        current_USER = ""
        Me.WindowState = FormWindowState.Normal
        Me.Width = 616
        Me.Height = 438
        Me.FormBorderStyle = Windows.Forms.FormBorderStyle.FixedSingle
        Me.MaximizeBox = False
        Me.Refresh()
    End Sub

    Public Sub resetUser()
        lbl_USER.Text = getName(current_USER)
    End Sub



    'ADD SCHEDULE PANEL

    Private Sub btn_panelAS_back_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btn_panelAS_back.Click
        Me.Text = "Room Sched - Menu"
        Panel_MainMenu.Visible = True
        Panel_AddSched.Visible = False
    End Sub

    Private Sub btn_NewSched_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btn_NewSched.Click
        Me.Text = "Room Sched - New Schedule"
        Panel_MainMenu.Visible = False
        Panel_AddSched.Visible = True
    End Sub

    Public Sub updateFoundRooms()
        Dim _from As Date = New Date(date_from.Value.Year, date_from.Value.Month, date_from.Value.Day, date_time_from.Value.Hour, date_time_from.Value.Minute, date_time_from.Value.Second)
        Dim _to As Date = New Date(date_to.Value.Year, date_to.Value.Month, date_to.Value.Day, date_time_to.Value.Hour, date_time_to.Value.Minute, date_time_to.Value.Second)
        ComboBox_FoundRooms.ResetText()
        ComboBox_FoundRooms.Items.Clear()

        If (_to.Ticks - _from.Ticks) <= 0 Then
            MessageBox.Show("Invalid Date", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
        ElseIf _from.Ticks < dateNow.Ticks Then
            MessageBox.Show("Invalid Date", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Else

            For Each room In findAvailable(_from.Ticks, _to.Ticks)
                ComboBox_FoundRooms.Items.Add(room)
            Next

            If ComboBox_FoundRooms.Items.Count > 0 Then
                ComboBox_FoundRooms.Text = ComboBox_FoundRooms.Items(0)
                MessageBox.Show(ComboBox_FoundRooms.Items.Count.ToString + " Available Rooms Found", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
            Else
                MessageBox.Show("No Available Room Found", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If
        End If
    End Sub

    Private Sub btn_FIND_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btn_FIND.Click
        LBL_LOADING.Visible = True
        Application.DoEvents()
        ComboBox_FoundRooms.ResetText()
        updateFoundRooms()
        ComboBox_FoundRooms.Update()
        LBL_LOADING.Visible = False
    End Sub

    Private Sub btn_ADDSCHED_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btn_ADDSCHED.Click
        LBL_LOADING.Visible = True
        Application.DoEvents()
        Dim _from As Date = New Date(date_from.Value.Year, date_from.Value.Month, date_from.Value.Day, date_time_from.Value.Hour, date_time_from.Value.Minute, date_time_from.Value.Second)
        Dim _to As Date = New Date(date_to.Value.Year, date_to.Value.Month, date_to.Value.Day, date_time_to.Value.Hour, date_time_to.Value.Minute, date_time_to.Value.Second)

        If ComboBox_FoundRooms.Items.Count = 0 Then
            MessageBox.Show("No Selected Room", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
        ElseIf (_to.Ticks - _from.Ticks) <= 0 Then
            MessageBox.Show("Invalid Date", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
        ElseIf _from.Ticks < dateNow.Ticks Then
            MessageBox.Show("Invalid Date", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Else
            Try
                If (_to.Ticks - _from.Ticks) <= 0 Then
                    MessageBox.Show("Invalid Date", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
                Else
                    addSchedule(ComboBox_FoundRooms.SelectedItem.ToString, _from.Ticks.ToString, _to.Ticks.ToString, current_USER)
                    initRightSidePanel()
                    initRoomsListBox()
                    ComboBox_FoundRooms.ResetText()
                    ComboBox_FoundRooms.Items.Clear()
                    MessageBox.Show("Schedule Added", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
                End If
            Catch ex As Exception
                MessageBox.Show("Invalid Room", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End Try
        End If
        LBL_LOADING.Visible = False
    End Sub

    'END ADD SCHEDULE PANEL


    'ADD ROOM PANEL

    Private Sub btn_BackAddRoom_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btn_BackAddRoom.Click
        Me.Text = "Room Sched - Menu"
        txt_ROOMID.Clear()
        RText_RoomDesc.Clear()
        Panel_MainMenu.Visible = True
        Panel_AddRoom.Visible = False
    End Sub

    Private Sub btn_NewRoom_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btn_NewRoom.Click
        Me.Text = "Room Sched - Add Room"
        Panel_MainMenu.Visible = False
        Panel_AddRoom.Visible = True
    End Sub

    Private Sub btn_AddNewRoom_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btn_AddNewRoom.Click
        LBL_LOADING.Visible = True
        Application.DoEvents()
        If txt_ROOMID.Text.Contains(" ") Then
            MessageBox.Show("Remove Spaces in Room ID", "Room", MessageBoxButtons.OK, MessageBoxIcon.Information)
        ElseIf txt_ROOMID.Text = "" Then
            MessageBox.Show("Add a Room ID", "Room", MessageBoxButtons.OK, MessageBoxIcon.Information)
        Else
            Dim output As String = addRoom(txt_ROOMID.Text, RText_RoomDesc.Text)
            initRightSidePanel()
            txt_ROOMID.Clear()
            RText_RoomDesc.Clear()
            MessageBox.Show(output, "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If
        LBL_LOADING.Visible = False
    End Sub

    'END ADD ROOM PANEL


    'ROOMS AND SCHEDULES PANEL

    Private Sub btn_backRooms_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btn_backRooms.Click
        Me.Text = "Room Sched - Menu"
        Panel_MainMenu.Visible = True
        Panel_Rooms.Visible = False
    End Sub

    Private Sub btn_Rooms_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btn_Rooms.Click
        Me.Text = "Room Sched - Room Schedules"
        Panel_MainMenu.Visible = False
        Panel_Rooms.Visible = True
    End Sub

    'END ROOMS AND SCHEDULES PANEL

    Private Sub btn_LOGOUT_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btn_LOGOUT.Click
        logOut()
    End Sub

    Private Sub btn_RemoveSched_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btn_RemoveSched.Click
        Dim result_ As New DialogResult
        result_ = MessageBox.Show("Delete the Schedule?", "Delete", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
        LBL_LOADING.Visible = True
        Application.DoEvents()
        Try
            If result_ = DialogResult.Yes Then
                removeSchedule(Integer.Parse(DataGridView_ROOMS.SelectedCells(0).Value))
                initRoomsListBox()
                initRightSidePanel()
                MessageBox.Show("Schedule Deleted", "Info", MessageBoxButtons.OK, MessageBoxIcon.Information)
            End If

        Catch ex As Exception
            MessageBox.Show("Please Select Schedule to delete", "No Selection", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End Try
        LBL_LOADING.Visible = False
    End Sub

    ' LOGIN

    Private Sub btn_LOGIN_Click(ByVal sender As Object, ByVal e As System.EventArgs) Handles btn_LOGIN.Click
        If authenticate(txt_USERNAME.Text, txt_PASSWORD.Text) Then
            current_USER = txt_USERNAME.Text
            logInAuthorized()
        Else
            MessageBox.Show("Wrong Username/Password", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning)
        End If
    End Sub

    Private Sub btn_launch_createAcc_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btn_launch_createAcc.Click
        Me.Text = "Room Sched - Create Account"
        Panel_CreateAccount.Visible = True
        Panel_LOGINFIELDS.Visible = False
        txt_USERNAME.Clear()
        txt_PASSWORD.Clear()
    End Sub

    ' END LOGIN


    ' CREATE ACCOUNT

    Private Sub btn_CREATEPANELBACK_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btn_CREATEPANELBACK.Click
        Me.Text = "Room Sched - Login"
        Panel_CreateAccount.Visible = False
        Panel_LOGINFIELDS.Visible = True
        txt_createName.Clear()
        txt_createUSERNAME.Clear()
        txt_createPASSWORD.Clear()
        txt_createCONFIRMPASSWORD.Clear()
    End Sub

    Private Sub btn_CreateAccount_Click(ByVal sender As System.Object, ByVal e As System.EventArgs) Handles btn_CreateAccount.Click
        Dim validAcc As Boolean = checkCreateAccFields()
        If validAcc Then
            addAccount(txt_createUSERNAME.Text, txt_createPASSWORD.Text, txt_createName.Text)
            MessageBox.Show("Account Created", "Success", MessageBoxButtons.OK, MessageBoxIcon.Information)
            txt_createName.Clear()
            txt_createUSERNAME.Clear()
            txt_createPASSWORD.Clear()
            txt_createCONFIRMPASSWORD.Clear()
        End If
    End Sub

    Public Function checkCreateAccFields() As Boolean
        Dim C_NAME As Boolean = True
        Dim C_USERNAME As Boolean = True
        Dim C_PASSWORD As Boolean = True
        Dim C_CPASSWORD As Boolean = True

        If txt_createName.Text = "" Then
            C_NAME = False
            MessageBox.Show("Empty Field", "Invalid Name", MessageBoxButtons.OK, MessageBoxIcon.Information)
        ElseIf txt_createUSERNAME.Text = "" Or txt_createUSERNAME.Text.Contains(" ") Then
            C_USERNAME = False
            txt_createUSERNAME.ForeColor = Color.Red
            MessageBox.Show("Alpha-numericals only", "Invalid Username", MessageBoxButtons.OK, MessageBoxIcon.Information)
        ElseIf Not validUsername(txt_createUSERNAME.Text) Then
            C_USERNAME = False
            txt_createUSERNAME.ForeColor = Color.Red
            MessageBox.Show("Username Already Exist", "Invalid Username", MessageBoxButtons.OK, MessageBoxIcon.Information)
        ElseIf txt_createPASSWORD.Text = "" Or txt_createCONFIRMPASSWORD.Text = "" Then
            C_PASSWORD = False
            C_CPASSWORD = False
            MessageBox.Show("Empty Field", "Invalid Password", MessageBoxButtons.OK, MessageBoxIcon.Information)
        ElseIf Not txt_createCONFIRMPASSWORD.Text = txt_createPASSWORD.Text Then
            C_PASSWORD = False
            C_CPASSWORD = False
            txt_createPASSWORD.ForeColor = Color.Red
            txt_createCONFIRMPASSWORD.ForeColor = Color.Red
            MessageBox.Show("Password didn't match", "Invalid Password", MessageBoxButtons.OK, MessageBoxIcon.Information)
        End If

        Return C_NAME And C_USERNAME And C_PASSWORD And C_CPASSWORD

    End Function

    ' END CREATE ACCOUNT




    ' UX: this is to prevent adding unwanted timestamps in ADD SCHEDULE
    Private Sub date_from_ValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles date_from.ValueChanged
        ComboBox_FoundRooms.ResetText()
        ComboBox_FoundRooms.Items.Clear()
    End Sub
    Private Sub date_time_from_ValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles date_time_from.ValueChanged
        ComboBox_FoundRooms.ResetText()
        ComboBox_FoundRooms.Items.Clear()
    End Sub
    Private Sub date_time_to_ValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles date_time_to.ValueChanged
        ComboBox_FoundRooms.ResetText()
        ComboBox_FoundRooms.Items.Clear()
    End Sub
    Private Sub date_to_ValueChanged(ByVal sender As Object, ByVal e As System.EventArgs) Handles date_to.ValueChanged
        ComboBox_FoundRooms.ResetText()
        ComboBox_FoundRooms.Items.Clear()
    End Sub

    ' UX: this is to reset invalid fields in CREATE ACCOUNT
    Private Sub txt_createName_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txt_createName.GotFocus
        txt_createName.ForeColor = Color.Black
    End Sub
    Private Sub txt_createUSERNAME_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txt_createUSERNAME.GotFocus
        txt_createUSERNAME.ForeColor = Color.Black
    End Sub
    Private Sub txt_createPASSWORD_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txt_createPASSWORD.GotFocus
        txt_createCONFIRMPASSWORD.ForeColor = Color.Black
        txt_createPASSWORD.ForeColor = Color.Black
    End Sub
    Private Sub txt_createCONFIRMPASSWORD_GotFocus(ByVal sender As Object, ByVal e As System.EventArgs) Handles txt_createCONFIRMPASSWORD.GotFocus
        txt_createCONFIRMPASSWORD.ForeColor = Color.Black
        txt_createPASSWORD.ForeColor = Color.Black
    End Sub


    Private Sub txt_USERNAME_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txt_USERNAME.KeyDown
        If e.KeyCode = 13 Then
            txt_PASSWORD.Focus()
        End If
    End Sub

    Private Sub txt_PASSWORD_KeyDown(ByVal sender As Object, ByVal e As System.Windows.Forms.KeyEventArgs) Handles txt_PASSWORD.KeyDown
        If e.KeyCode = 13 Then
            If authenticate(txt_USERNAME.Text, txt_PASSWORD.Text) Then
                current_USER = txt_USERNAME.Text
                logInAuthorized()
            Else
                MessageBox.Show("Wrong Username/Password", "Access Denied", MessageBoxButtons.OK, MessageBoxIcon.Warning)
            End If
        End If
    End Sub


    ' For clock
    Private Sub Timer1_Tick(ByVal sender As Object, ByVal e As System.EventArgs) Handles Timer1.Tick
        lbl_CLOCK.Text = Format(Now, "hh:mm tt")
        lbl_dateNow.Text = Format(Now, "dddd MMMM dd, yyyy")
    End Sub


End Class
