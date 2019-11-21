﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.ServiceModel;
using MainWindow.ServiceChat;
using MainWindow.Properties;

namespace MainWindow
{

	public partial class UserWindow : Form, IServiceCallback
	{
		private SingInWindow _singInWindow;
		private string _userName;
		private List<ChatUser> _allUsers;
		private Point _lastPoint;
		private UserListItem _currentUserItem;
		private UserListItem[] _userListItems;
		private ServiceClient _client;

		public UserWindow(string userName, SingInWindow singInWindow)
		{
			InitializeComponent();
			
			_currentUserItem = new UserListItem(this);
			_userName = userName;
			_singInWindow = singInWindow;
			this.userName.Text = userName;
			_allUsers = new List<ChatUser>();
		}


        public void MsgCallback(string fromUser, string toUser, string msg)
		{
			if (toUser != _userName)
				return;

			for (int i = 0; i < _allUsers.Count; ++i)
			{
				if (_allUsers[i].userName == fromUser)
				{
					MessageItem msgItem = new MessageItem(msg, DateTime.Now.ToString(),
							_userName + fromUser + DateTime.Now.ToString(), fromUser);
					_allUsers[i].msgItems.Add(msgItem);

					if (_currentUserItem.UserName == fromUser)
					{
						this.msgFlowPanel.Controls.Add(msgItem);
					}
					else
					{
						_allUsers.ToArray()[i].haveMsg = true;
						for (int k = 0; k < _userListItems.Length; ++k)
						{
							if (_userListItems[k].UserName == fromUser)
							{
								_userListItems[k].HaveMsgImage = Resources.haveMsg;
								break;
							}
						}
					}
					break;
				}
			}
		}

		public void ConnectUserCallback(string userName)
		{
			if (userName == _userName)
				return;

			bool flag = true;
			for (int i = 0; i < _allUsers.Count; ++i)
			{
				if (_allUsers.ToArray()[i].userName == userName)
				{
					ChatUser newCU = new ChatUser();
					newCU = _allUsers.ToArray()[i];
					newCU.isConnected = true;
					_allUsers.Remove(_allUsers[i]);
					_allUsers.Insert(i, newCU);

					flag = false;
					break;
				}
			}

			if (flag)
			{
				ChatUser cu = new ChatUser { userName = userName, isConnected = true, haveMsg = false };
				_allUsers.Add(cu);
			}
			PopulateInemsUser();

		}

		public void DisconnectUserCallback(string userName)
		{
			if (userName == _userName)
				return;

			for (int i = 0; i < _allUsers.Count; ++i)
			{
				if (_allUsers[i].userName == userName)
				{
					ChatUser ncu = new ChatUser();
					ncu = _allUsers.ToArray()[i];
					ncu.isConnected = false;
					_allUsers.Remove(_allUsers[i]);
					_allUsers.Insert(i, ncu);
					break;
				}
			}

			PopulateInemsUser();
		}

		//--------------------------------
		// ----------- get ---------------
		//--------------------------------

		public string GetUserName()
		{
			return _userName;
		}

		public List<string> GetAllUsersName()
		{
			List<string> usersName = new List<string> { };
			foreach (ChatUser cu in _allUsers)
			{
				usersName.Add(cu.userName);
			}

			return usersName;
		}

		public bool ThisUserIsConnect(string username)
		{
			bool isConnect = false;
			for (int i = 0; i < _allUsers.Count; ++i)
			{
				if (_allUsers[i].userName == username)
				{
					isConnect = _allUsers[i].isConnected;
					break;
				}
			}

			return isConnect;
		}

		//--------------------------------
		// ---------- chat logic ---------
		//--------------------------------

		public void ClickUserItem(UserListItem item)
		{
			СhangeCurrentUserItem(item);

			for (int i = 0; i < _allUsers.Count; ++i)
			{
				if (_allUsers[i].userName == item.UserName)
				{
					// Взять сообщения с сервиса и отобрпзить их(сохранить как items)
					DrowMsg(_allUsers[i]);
					break;
				}
			}
		}



		//--------------------------------
		//----------- design -------------
		//--------------------------------

		private void UserWindow_Load(object sender, EventArgs e)
		{

			_client = new ServiceClient(new System.ServiceModel.InstanceContext(this));
			string[] allUserArr = allUserArr = _client.Connect(_userName);

			for (int i = 0; i < allUserArr.Length; ++i)
			{
				string[] words = allUserArr[i].Split(new char[] { '&' });
				if (_userName != words[0])
				{
					ChatUser newCU = new ChatUser();
					newCU.userName = words[0];
					newCU.isConnected = ('1' == words[1][0]) ? true : false;
					newCU.haveMsg = ('1' == words[1][1]) ? true : false;
					newCU.msgItems = new List<MessageItem>();
					_allUsers.Add(newCU);
				}
			}

			PopulateInemsUser();
		}

		// Отрисовка списка всех пользователей
		private void PopulateInemsUser()
		{
			this.PanelListUsers.Controls.Clear();
			
			_userListItems = new UserListItem[_allUsers.Count];
			
			for (int i = 0; i < _userListItems.Length; ++i)
			{
				_userListItems[i] = new UserListItem(this);
				_userListItems[i].UserName = _allUsers[i].userName;


				if (_allUsers[i].isConnected)
					_userListItems[i].ConnectedImage = Resources.Circle_Green;
				else
					_userListItems[i].ConnectedImage = Resources.Circle_Red;


				if (_allUsers[i].haveMsg)
					_userListItems[i].HaveMsgImage = Resources.haveMsg;
				else
					_userListItems[i].HaveMsgImage = Resources.Tick;
				
				this.PanelListUsers.Controls.Add(_userListItems[i]);
			}

			//СhangeCurrentUserItem(_currentUserItem);
		}

		private void PopulateInemsUserNew()
		{
			this.PanelListUsers.Controls.Clear();

			//_userListItems = new UserListItem[_allUsers.Count];
			//for (int i = 0; i < _userListItems.Length; ++i)
			//{
			//	_userListItems[i] = new UserListItem(this);
			//	_userListItems[i].UserName = _allUsers[i].userName;


			//	if (_allUsers[i].isConnected)
			//		_userListItems[i].ConnectedImage = Resources.Circle_Green;
			//	else
			//		_userListItems[i].ConnectedImage = Resources.Circle_Red;


			//	if (_allUsers[i].haveMsg)
			//		_userListItems[i].HaveMsgImage = Resources.haveMsg;
			//	else
			//		_userListItems[i].HaveMsgImage = Resources.Tick;

			//	this.PanelListUsers.Controls.Add(_userListItems[i]);
			//}

			//СhangeCurrentUserItem(_currentUserItem);
		}



		private void DrowMsg(ChatUser cu)
		{
			this.msgFlowPanel.Controls.Clear();
			for (int i = 0; i < cu.msgItems.Count; ++i)
			{
				this.msgFlowPanel.Controls.Add(cu.msgItems[i]);
			}
		}

		private void СhangeCurrentUserItem(UserListItem item)
		{
			_currentUserItem.SetBackColor(Color.White);
			_currentUserItem.clickAtThis = false;
			_currentUserItem = item;
			item.SetBackColor(Color.FromArgb(132, 133, 235));
			item.HaveMsgImage = Resources.Tick;
		}

		private void topblokAuth_MouseMove(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				this.Left += e.X - _lastPoint.X;
				this.Top += e.Y - _lastPoint.Y;
			}
		}

		private void topblokAuth_MouseDown(object sender, MouseEventArgs e)
		{
			_lastPoint = new Point(e.X, e.Y);
		}

		private void closeButton_Click(object sender, EventArgs e)
		{
			this.Close();
			_client.Disconnect(_userName);
			_singInWindow.Close();
		}

		private void closeButton_MouseEnter(object sender, EventArgs e)
		{
			this.closeButton.BackColor = Color.Red;
		}

		private void closeButton_MouseLeave(object sender, EventArgs e)
		{
			this.closeButton.BackColor = Color.FromArgb(62, 128, 182);
		}

		private void buttonMinimized_Click(object sender, EventArgs e)
		{
			this.WindowState = FormWindowState.Minimized;
		}

		private void buttonMinimized_MouseEnter(object sender, EventArgs e)
		{
			this.buttonMinimized.BackColor = Color.FromArgb(131, 175, 230);
		}

		private void buttonMinimized_MouseLeave(object sender, EventArgs e)
		{
			this.buttonMinimized.BackColor = Color.FromArgb(62, 128, 182);
		}

		private void buttonLogOut_Click(object sender, EventArgs e)
		{
			this.Close();
			_client.Disconnect(_userName);
			_singInWindow.Show();
		}

		private void msgButton_MouseClick(object sender, MouseEventArgs e)
		{
			if (this.msgTextBox.Text == "")
				return;

			string date = DateTime.Now.ToString();
			
			MessageItem msg = new MessageItem(
				this.msgTextBox.Text,
				date,
				_userName + _currentUserItem.UserName + date,
				_currentUserItem.UserName
			);

			this.msgFlowPanel.Controls.Add(msg);

			for (int i = 0; i < _allUsers.Count; ++i)
			{
				if (_allUsers[i].userName == _currentUserItem.UserName)
				{
					_allUsers[i].msgItems.Add(msg);
					break;
				}
			}

			_client.SendMsg(_userName, _currentUserItem.UserName, this.msgTextBox.Text);
			this.msgTextBox.Text = "";
		}
	}
}
