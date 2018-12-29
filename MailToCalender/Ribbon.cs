using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using Office = Microsoft.Office.Core;
using Outlook = Microsoft.Office.Interop.Outlook;
using Word = Microsoft.Office.Interop.Word;
using AppointmentParser;
using System.Windows.Forms;

// TODO:  リボン (XML) アイテムを有効にするには、次の手順に従います。

// 1: 次のコード ブロックを ThisAddin、ThisWorkbook、ThisDocument のいずれかのクラスにコピーします。

//  protected override Microsoft.Office.Core.IRibbonExtensibility CreateRibbonExtensibilityObject()
//  {
//      return new Ribbon();
//  }

// 2. ボタンのクリックなど、ユーザーの操作を処理するためのコールバック メソッドを、このクラスの
//    "リボンのコールバック" 領域に作成します。メモ: このリボンがリボン デザイナーからエクスポートされたものである場合は、
//    イベント ハンドラー内のコードをコールバック メソッドに移動し、リボン拡張機能 (RibbonX) のプログラミング モデルで
//    動作するように、コードを変更します。

// 3. リボン XML ファイルのコントロール タグに、コードで適切なコールバック メソッドを識別するための属性を割り当てます。  

// 詳細については、Visual Studio Tools for Office ヘルプにあるリボン XML のドキュメントを参照してください。


namespace MailToCalender
{
    [ComVisible(true)]
    public class Ribbon : Office.IRibbonExtensibility
    {
        private Office.IRibbonUI ribbon;

        public Ribbon()
        {
        }

        /// <summary>
        /// 選択中のOutlookメールアイテムを取得
        /// 参考:<https://stackoverflow.com/questions/10935611/retrieve-current-email-body-in-outlook>
        /// </summary>
        /// <typeparam name="ItemType"></typeparam>
        /// <returns></returns>
        public ItemType GetSelectedItem<ItemType>() where ItemType : class
        {
            Outlook.Explorer explorer = Globals.ThisAddIn.Application.ActiveExplorer();
            Outlook.Selection selection = explorer.Selection;

            if (selection.Count <= 0)
                return null; 

            object selectedItem = selection[1];
            return selectedItem as ItemType;// Outlook.MailItem;
        }

        /// <summary>
        /// 選択中のメール本文の選択文字列を取得
        /// 参考:<https://www.codeproject.com/Questions/325619/Copy-Selected-text-from-message-with-outlook-Add-i>
        /// </summary>
        public string GetSelectedTextOnMailItem(Outlook.MailItem mailItem)
        {
            Outlook.Inspector inspector = mailItem.GetInspector;
            Word.Document document = (Word.Document)inspector.WordEditor;
            return document.Application.Selection.Text;
        }

        /// <summary>
        /// メールの選択文字列から予定を抽出し予定作成ダイアログを開く
        /// </summary>
        /// <param name="control"></param>
        public void OnScheduleEmail(Office.IRibbonControl control)
        {
            Outlook.MailItem mailItem = null;
            string text = null;
            try
            {
                mailItem = GetSelectedItem<Outlook.MailItem>();
                if (mailItem == null)
                    return;

                //選択中のテキストを取得
                text = GetSelectedTextOnMailItem(mailItem);
            }catch(Exception ex)
            {
                ShowError("メールまたは選択テキストの取得に失敗しました。", ex);
                return;
            }

            Appointment appointment = null;
            string place = null;
            try
            {
                //テキストから予定を解析
                appointment = new TextAppointmentParser().Parse(text);
                place = new TextPlaceParser().Parse(text);
            }catch(Exception ex)
            {
                ShowError("予定の解析に失敗しました。", ex);
                return;
            }

            try
            {
                //予定を作成
                Outlook.AppointmentItem apptItem = CreateAppointmentItemOnDefaultCalendar(mailItem, appointment, place);
                //予定ダイアログをモードレスで表示する
                apptItem.Display(false);
            }
            catch (Exception ex)
            {
                ShowError("予定の作成に失敗しました。\n", ex);
            }
        }

        /// <summary>
        /// 既定の予定表に予定を作成(保存はされてはいない)
        /// </summary>
        /// <param name="mailItem"></param>
        /// <param name="appointment"></param>
        /// <param name="place"></param>
        /// <returns></returns>
        private static Outlook.AppointmentItem CreateAppointmentItemOnDefaultCalendar(Outlook.MailItem mailItem, Appointment appointment, string place)
        {
            Outlook.MAPIFolder defaultCalendar = Globals.ThisAddIn.Application.Session.GetDefaultFolder(Outlook.OlDefaultFolders.olFolderCalendar);
            Outlook.AppointmentItem apptItem = defaultCalendar.Items.Add(Outlook.OlItemType.olAppointmentItem)
                                               as Outlook.AppointmentItem;
            apptItem.Start = appointment.Start;
            apptItem.End = appointment.End;
            apptItem.Subject = mailItem.Subject;
            apptItem.Location = place;
            apptItem.AllDayEvent = appointment.AllDay;
            apptItem.Attachments.Add(mailItem);
            return apptItem;
        }

        /// <summary>
        /// エラーを表示
        /// </summary>
        /// <param name="text"></param>
        /// <param name="ex"></param>
        public void ShowError(string text, Exception ex)
        {
            if(ex != null)
                text += "\n" + ex.Message;

            MessageBox.Show(text + "\n" + ex.Message, "MailToCalendarツール", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        #region IRibbonExtensibility のメンバー

        public string GetCustomUI(string ribbonID)
        {
            return GetResourceText("MailToCalender.Ribbon.xml");
        }

        #endregion

        #region リボンのコールバック
        //ここでコールバック メソッドを作成します。コールバック メソッドの追加について詳しくは https://go.microsoft.com/fwlink/?LinkID=271226 をご覧ください

        public void Ribbon_Load(Office.IRibbonUI ribbonUI)
        {
            this.ribbon = ribbonUI;
        }

        #endregion

        #region ヘルパー

        private static string GetResourceText(string resourceName)
        {
            Assembly asm = Assembly.GetExecutingAssembly();
            string[] resourceNames = asm.GetManifestResourceNames();
            for (int i = 0; i < resourceNames.Length; ++i)
            {
                if (string.Compare(resourceName, resourceNames[i], StringComparison.OrdinalIgnoreCase) == 0)
                {
                    using (StreamReader resourceReader = new StreamReader(asm.GetManifestResourceStream(resourceNames[i])))
                    {
                        if (resourceReader != null)
                        {
                            return resourceReader.ReadToEnd();
                        }
                    }
                }
            }
            return null;
        }

        #endregion
    }
}
