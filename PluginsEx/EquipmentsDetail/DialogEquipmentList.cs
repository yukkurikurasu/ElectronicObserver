using ElectronicObserver.Data;
using ElectronicObserver.Resource;
using ElectronicObserver.Window.Support;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading.Tasks;
using System.Windows.Forms;

using ElectronicObserver;
using ElectronicObserver.Window;

namespace EquipmentsDetail
{
	public partial class DialogEquipmentList : Form {


        public string ConfigFile;
        public List<int> Filters;
		private DataGridViewCellStyle CSDefaultLeft, CSDefaultRight, 
			CSUnselectableLeft, CSUnselectableRight;


		public DialogEquipmentList() {
			this.SuspendLayoutForDpiScale();

			InitializeComponent();

            ConfigFile = Application.StartupPath + "\\Settings\\EquipmentsDetail.xml";
            Filters = new List<int>();
			ControlHelper.SetDoubleBuffered( EquipmentView );

            Font = ElectronicObserver.Utility.Configuration.Config.UI.MainFont;

			foreach ( DataGridViewColumn column in EquipmentView.Columns ) {
				column.MinimumWidth = 2;
			}



			#region CellStyle

			CSDefaultLeft = new DataGridViewCellStyle();
			CSDefaultLeft.Alignment = DataGridViewContentAlignment.MiddleLeft;
			CSDefaultLeft.BackColor = SystemColors.Control;
			CSDefaultLeft.Font = Font;
			CSDefaultLeft.ForeColor = SystemColors.ControlText;
			CSDefaultLeft.SelectionBackColor = Color.FromArgb( 0xFF, 0xFF, 0xCC );
			CSDefaultLeft.SelectionForeColor = SystemColors.ControlText;
			CSDefaultLeft.WrapMode = DataGridViewTriState.False;

			CSDefaultRight = new DataGridViewCellStyle( CSDefaultLeft );
			CSDefaultRight.Alignment = DataGridViewContentAlignment.MiddleRight;

			CSUnselectableLeft = new DataGridViewCellStyle( CSDefaultLeft );
			CSUnselectableLeft.SelectionForeColor = CSUnselectableLeft.ForeColor;
			CSUnselectableLeft.SelectionBackColor = CSUnselectableLeft.BackColor;

			CSUnselectableRight = new DataGridViewCellStyle( CSDefaultRight );
			CSUnselectableRight.SelectionForeColor = CSUnselectableRight.ForeColor;
			CSUnselectableRight.SelectionBackColor = CSUnselectableRight.BackColor;


			EquipmentView.DefaultCellStyle = CSDefaultRight;
			EquipmentView_Name.DefaultCellStyle = CSDefaultLeft;
			EquipmentView.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

			int headerHeight = this.GetDpiHeight( 23 );
			EquipmentView.ColumnHeadersHeight = headerHeight;
			EquipmentView.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;

			#endregion

			this.ResumeLayoutForDpiScale();
		}

		private void DialogEquipmentList_Load( object sender, EventArgs e ) {

            LoadConfig();

			UpdateView();

			this.Icon = ResourceManager.ImageToIcon( ResourceManager.Instance.Icons.Images[(int)ResourceManager.IconContent.FormEquipmentList] );

		}

		private void EquipmentView_CellFormatting( object sender, DataGridViewCellFormattingEventArgs e ) {

            if (e.ColumnIndex == EquipmentView_Icon.Index)
            {
                e.Value = ResourceManager.Instance.Equipments.Images[(int)e.Value];
                e.FormattingApplied = true;
            }
            if (e.ColumnIndex == ColLevel.Index)
            {
                if (e.Value.ToString() == "")
                    return;
                e.Value = "+" + e.Value.ToString();
                e.FormattingApplied = true;
            }
		}

		/// <summary>
		/// 一覧ビューを更新します。
		/// </summary>
		private void UpdateView() {
            bool filtered = checkBox1.Checked;
			var ships = KCDatabase.Instance.Ships.Values;
            var equipments = KCDatabase.Instance.Equipments.Values.OrderBy<EquipmentData, int>(e => e.MasterEquipment.IconType);
			var masterEquipments = KCDatabase.Instance.MasterEquipments;
            int masterCount = masterEquipments.Values.Count(eq => !eq.IsAbyssalEquipment && (!filtered && !Filters.Contains(eq.EquipmentID)));

			var allCount = new Dictionary<int, int>( masterCount );
            
			//全個数計算
			foreach ( var e in equipments ) {

                if (Filters.Contains(e.EquipmentID) && filtered)
                    continue;
				if ( !allCount.ContainsKey( e.EquipmentID ) ) {
					allCount.Add( e.EquipmentID, 1 );

				} else {
					allCount[e.EquipmentID]++;
				}
			}

			var remainCount = new Dictionary<int, int>( allCount );

			//剰余数計算
            foreach (var ship in ships)
            {
                foreach (var eq in ship.AllSlotInstanceMaster)
                {
                    if (eq == null) continue;
                    if (remainCount.ContainsKey(eq.EquipmentID))
                        remainCount[eq.EquipmentID]--;
                }
            }

			//表示処理
			EquipmentView.SuspendLayout();

			EquipmentView.Enabled = false;
			EquipmentView.Rows.Clear();

			var rows = new List<DataGridViewRow>( allCount.Count );
			var ids = allCount.Keys;



            foreach (int id in ids)
            {
                StringBuilder sb = new StringBuilder();
                var Equipment = masterEquipments[id];
                sb.AppendFormat("{0} {1}\r\n", Equipment.CategoryTypeInstance.Name, Equipment.Name);
                if (Equipment.Firepower != 0)
                    sb.AppendFormat("火力 {0}{1}\r\n", Equipment.Firepower > 0 ? "+" : "", Equipment.Firepower);
                if (Equipment.Torpedo != 0)
                    sb.AppendFormat("雷装 {0}{1}\r\n", Equipment.Torpedo > 0 ? "+" : "", Equipment.Torpedo);
                if (Equipment.AA != 0)
                    sb.AppendFormat("対空 {0}{1}\r\n", Equipment.AA > 0 ? "+" : "", Equipment.AA);
                if (Equipment.Armor != 0)
                    sb.AppendFormat("装甲 {0}{1}\r\n", Equipment.Armor > 0 ? "+" : "", Equipment.Armor);
                if (Equipment.ASW != 0)
                    sb.AppendFormat("対潜 {0}{1}\r\n", Equipment.ASW > 0 ? "+" : "", Equipment.ASW);
                if (Equipment.Evasion != 0)
                    sb.AppendFormat("回避 {0}{1}\r\n", Equipment.Evasion > 0 ? "+" : "", Equipment.Evasion);
                if (Equipment.LOS != 0)
                    sb.AppendFormat("索敵 {0}{1}\r\n", Equipment.LOS > 0 ? "+" : "", Equipment.LOS);
                if (Equipment.Accuracy != 0)
                    sb.AppendFormat("命中 {0}{1}\r\n", Equipment.Accuracy > 0 ? "+" : "", Equipment.Accuracy);
                if (Equipment.Bomber != 0)
                    sb.AppendFormat("爆装 {0}{1}\r\n", Equipment.Bomber > 0 ? "+" : "", Equipment.Bomber);
                sb.AppendLine("(右键点击查看图鉴)");

                var eqs = KCDatabase.Instance.Equipments.Values.Where(eq => eq.EquipmentID == id).OrderBy(e => (e.Level + e.AircraftLevel * 10));
                var countlist = new Dictionary<int, DetailCounter>();

                foreach (var eq in eqs)
                {
                    int Key = DetailCounter.CalculateID(eq);
                    DetailCounter c;
                    if (!countlist.ContainsKey(Key))
                    {
                        DetailCounter dc = new DetailCounter(eq.Level, eq.AircraftLevel);
                        countlist.Add(dc.ID, dc);
                    }
                    c = countlist[Key];
                    c.countAll++;
                    c.countRemain++;
                    c.countRemainPrev++;
                }

                //装備艦集計
                foreach (var ship in KCDatabase.Instance.Ships.Values)
                {
                    foreach (var eq in ship.AllSlotInstance.Where(s => s != null && s.EquipmentID == id))
                    {
                        countlist[DetailCounter.CalculateID(eq)].countRemain--;
                    }
                    foreach (var c in countlist.Values)
                    {
                        if (c.countRemain != c.countRemainPrev)
                        {
                            int diff = c.countRemainPrev - c.countRemain;
                            c.equippedShips.Add(ship.NameWithLevel + (diff > 1 ? (" x" + diff) : ""));
                            c.countRemainPrev = c.countRemain;
                        }
                    }
                }
                bool FirstRow = true;
                foreach (var c in countlist.Values)
                {

                    bool FirstShipRow = true;

                    for (int cc = 0; cc < Math.Max(1, (c.equippedShips.Count + 4) / 5); cc++)
                    {
                        var row = new DataGridViewRow();
                        row.CreateCells(EquipmentView);

                        row.SetValues(
                            FirstRow ? id.ToString() : "",
                            FirstRow ? masterEquipments[id].IconType : 0,
                            FirstRow ? masterEquipments[id].Name : "",
                            FirstRow ? allCount[id].ToString() + "(" + remainCount[id].ToString() + ")" : "",
                            FirstShipRow ? c.level.ToString() : "",
                            FirstShipRow ? c.aircraftLevel.ToString() : "",
                            FirstShipRow ? c.countAll.ToString() + "(" + c.countRemain.ToString() + ")" : "",
                            GetShips(c.equippedShips, cc)
                            );
                        row.Cells[0].Tag = FirstRow ? 0 : id;
                        row.Cells[1].Tag = FirstRow ? 0 : 1;
                        row.Cells[2].Tag = FirstRow ? 0 : 1;
                        row.Cells[2].ToolTipText = FirstRow ? sb.ToString() : "";
                        row.Cells[3].Tag = FirstRow ? 0 : 1;
                        row.Cells[4].Tag = FirstShipRow ? 0 : 1;
                        row.Cells[5].Tag = FirstShipRow ? 0 : 1;
                        row.Cells[6].Tag = FirstShipRow ? 0 : 1;
                        row.Cells[7].Tag = FirstShipRow ? 0 : 1;
                        rows.Add(row);
                        FirstRow = false;
                        FirstShipRow = false;
                    }

                }

            }

			for ( int i = 0; i < rows.Count; i++ )
				rows[i].Tag = i;

			EquipmentView.Rows.AddRange( rows.ToArray() );

			//EquipmentView.Sort( EquipmentView_Name, ListSortDirection.Ascending );


			EquipmentView.Enabled = true;
			EquipmentView.ResumeLayout();

			if ( EquipmentView.Rows.Count > 0 )
				EquipmentView.CurrentCell = EquipmentView[0, 0];
            EquipmentView.Refresh();
		}

        string GetShips(List<string> ships, int index)
        {
            string List = "";
            for (int i = index * 5; i < ships.Count && i < index * 5 + 5; i++)
            {
                List += ships[i] + "  ";
            }
            return List;
        }

		private class DetailCounter : IIdentifiable {

			public int level;
            public int aircraftLevel;
			public int countAll;
			public int countRemain;
			public int countRemainPrev;

			public List<string> equippedShips;

			public DetailCounter( int lv, int aircraftLv ) {

                level =  lv;
                aircraftLevel = aircraftLv;
				countAll = 0;
				countRemainPrev = 0;
				countRemain = 0;
				equippedShips = new List<string>();
			}

			public static int CalculateID( int level, int aircraftLevel ) {
                return level + aircraftLevel * 10;
			}

			public static int CalculateID( EquipmentData eq ) {
				return CalculateID( eq.Level, eq.AircraftLevel );
			}

            public int ID { get { return CalculateID(level, aircraftLevel); } }
		}

		private void DialogEquipmentList_FormClosed( object sender, FormClosedEventArgs e ) {

			ResourceManager.DestroyIcon( Icon );
            SaveConfig();
		}

        private void EquipmentView_CellPainting(object sender, DataGridViewCellPaintingEventArgs e)
        {
            if (e.RowIndex < 0)
                return;
            e.AdvancedBorderStyle.Bottom = DataGridViewAdvancedCellBorderStyle.None;

            if (NoNeedTop(e))
            {
                e.AdvancedBorderStyle.Top = DataGridViewAdvancedCellBorderStyle.None;

            }
            else
            {
                e.AdvancedBorderStyle.Top = EquipmentView.AdvancedCellBorderStyle.Top;
            }
            e.Paint(e.ClipBounds, DataGridViewPaintParts.All);
            e.Handled = true;
        }

        bool NoNeedTop(DataGridViewCellPaintingEventArgs e)
        {
            if ((int)(EquipmentView.Rows[e.RowIndex].Cells[e.ColumnIndex].Tag) != 0)
            {
                return true;
            }
            return false;
        }

        public void LoadConfig()
        {
            try
            {
                if (System.IO.File.Exists(ConfigFile))
                {
                    XmlDocument doc = new XmlDocument();
                    doc.Load(ConfigFile);
                    var Root = doc.DocumentElement;
                    var str = Root.GetAttribute("DisabledEquipmentID");
                    Filters.Clear();
                    foreach(var s in str.Split(','))
                    {
                        int ID;
                        if (int.TryParse(s,out ID))
                        {
                            Filters.Add(ID);
                        }
                        
                    }
                }
            }
            catch
            {
            }
        }
        public void SaveConfig()
        {
            try
            {
                XmlDocument doc = new XmlDocument();
                if (!System.IO.File.Exists(ConfigFile))
                {
                    XmlElement xmlelem = doc.CreateElement("Config");
                    doc.AppendChild(xmlelem);
                }
                else
                {
                    doc.Load(ConfigFile);
                }
                var Root = doc.DocumentElement;
                Root.RemoveAll();
                var s = string.Join(",", Filters);

                Root.SetAttribute("DisabledEquipmentID", s);

                doc.Save(ConfigFile);
            }
            catch
            {
            }
        }

        private void button1_Click(object sender, EventArgs e)
        {
            FilterForm ff = new FilterForm();
            ff.SetFilter(Filters);
            ff.ShowDialog();
            UpdateView();
        }

        private void checkBox1_CheckedChanged(object sender, EventArgs e)
        {
            UpdateView();
        }

        private void EquipmentView_CellMouseClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Right)
            {
                if (e.ColumnIndex == 2)
                {
                    var cell = EquipmentView.Rows[e.RowIndex].Cells[0];
                    int ID;
                    if (int.TryParse(cell.Value.ToString(), out ID))                   
                    {
                        new ElectronicObserver.Window.Dialog.DialogAlbumMasterEquipment(ID).Show();
                    }
                }
            }
        }
	}
}
