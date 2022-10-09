using SerendipityLibrary;
using Serendipity.Geomsas;
using Serendipity.Forms;

namespace Serendipity
{
    public partial class MainForm
    {
        private void MenuStrip_ItemClicked(object sender, ToolStripItemClickedEventArgs e)
        {
            var item = e.ClickedItem;
            string text = item.Text;

            if (!text.Equals("파 일"))
            {
                panel4.Visible = false;
                panel3.Visible = true;
            }
        }

        #region FileMenu
        private void ExitToolStripMenuItem_Click(object sender, EventArgs e)
        {
            Application.Exit();
        }

        private void ConditonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panel3.Visible = false;
            panel4.Visible = true;
        }

        private async void 기본필터당번생성ToolStripMenuItem_Click(object sender, EventArgs e)
        {
            panel3.Visible = true;
            panel4.Visible = false;
            var gm = new Geomsa();
            FixedList.AddRange(Array.Empty<int>());
            ExceptList.AddRange(Array.Empty<int>());
            _cts = new CancellationTokenSource();
            var token = _cts.Token;
            StopButton.Enabled = true;
            PercentStatusLabel.Text = "데이터 준비중...";
            int cnt = (int)numericUpDown1.Value;
            await Task.Run(() => ReadCombine());
            toolStripProgressBar1.Maximum = cnt;

            try
            {
                var progress = new Progress<int>(value =>
                {
                    double val = value * 100.0 / cnt;
                    toolStripProgressBar1.PerformStep();
                    PercentStatusLabel.Text = val.ToString("F2") + "%";
                });
                var rst = await MakeOfPassedBasicConditon(cnt, progress, token);
                token.ThrowIfCancellationRequested();
                await Task.Delay(1000);
                toolStripProgressBar1.Value = 0;
                string path = Application.StartupPath + @"\DataFiles\alldang.csv";
                string s = string.Join(Environment.NewLine, rst.Select(x => SimpleData.ListToString(x)));
                File.WriteAllText(path, s);
                await Task.Delay(1000);

                //파일복사
                string destpath2 = @"D:\Git\GoldenLotto\GoldenLotto\bin\Debug\Data\alldang.csv";
                string destpath3 = @"D:\Git\PerfectLottoApp\PerfectLottoApp\bin\Debug\DataFiles\alldang.csv";
                string destpath4 = @"D:\Study\LottoMethod\LottoMethordApp\LottoMethordApp\bin\Debug\Data\alldang.csv";

                File.Copy(path, destpath2, true);
                File.Copy(path, destpath3, true);
                File.Copy(path, destpath4, true);

                MessageBox.Show("작업완료");
            }
            catch (OperationCanceledException)
            {
                MessageBox.Show("작업 취소됨.");
            }
            catch (IOException oe)
            {
                MessageBox.Show(oe.Message);
            }
            catch (Exception)
            {
                throw;
            }
            finally
            {
                _cts?.Dispose();
                StopButton.Enabled = false;
                toolStripProgressBar1.Value = 0;
                PercentStatusLabel.Text = string.Empty;
            }
        }
        #endregion

        #region DatabaseMenu
        //데이터 삽입
        private void InsertToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Application.OpenForms.OfType<DataInsertForm>().Any())
            {
                var frm = new DataInsertForm();
                frm.Show();
            }
        }
        //데이터 조회
        private void QueryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Application.OpenForms.OfType<DataQueryForm>().Any())
            {
                var frm = new DataQueryForm();
                frm.Show();
            }
        }
        //이격간격 데이터 조회
        private void GapQueryToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Application.OpenForms.OfType<GapQueryForm>().Any())
            {
                var frm = new GapQueryForm(_lastOrder);
                frm.Show();
            }
        }
        //데이터요소 3,5 검사
        private void SamohToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Application.OpenForms.OfType<SamOhDataForm>().Any())
            {
                var frm = new SamOhDataForm(_lastOrder);
                frm.Show();
            }
        }
        //데이터 직선,사선 검사
        private void SequenceGapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Application.OpenForms.OfType<SequenceGapForm>().Any())
            {
                var frm = new SequenceGapForm(_lastOrder);
                frm.Show();
            }
        }
        //데이터 행별출수 검사
        private void EachRowToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Application.OpenForms.OfType<HaengByeolForm>().Any())
            {
                var frm = new HaengByeolForm(_lastOrder);
                frm.Show();
            }
        }
        //데이터 회기, 주기검사
        private void HoikijukiToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Application.OpenForms.OfType<HoeikijukiForm>().Any())
            {
                var frm = new HoeikijukiForm(_lastOrder);
                frm.Show();
            }
        }
        //데이터 출현간격 검사
        private void ChulGapToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Application.OpenForms.OfType<ChulKankyeokForm>().Any())
            {
                var frm = new ChulKankyeokForm(_lastOrder);
                frm.Show();
            }
        }
        //데이터 이동평균 검사
        private void MoveAvgToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Application.OpenForms.OfType<MoveAvgForm>().Any())
            {
                var frm = new MoveAvgForm(_lastOrder);
                frm.Show();
            }
        }
        //데이터 캔들검사
        private void CandleToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Application.OpenForms.OfType<ChulsuCandleForm>().Any())
            {
                var frm = new ChulsuCandleForm(_lastOrder);
                frm.Show();
            }
        }
        //데이터 복합검사
        private void MultyCheckToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Application.OpenForms.OfType<MultyCheckForm>().Any())
            {
                var frm = new MultyCheckForm(_lastOrder);
                frm.Show();
            }
        }
        #endregion

        #region JohapMenu
        //데이터 단순회귀검사
        private void LinearToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Application.OpenForms.OfType<LinearForm>().Any())
            {
                var frm = new LinearForm(_lastOrder);
                frm.Show();
            }
        }
        //당번 출현회차 회귀검사
        private void RestCheckToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Application.OpenForms.OfType<DangbeonRegressionForm>().Any())
            {
                var frm = new DangbeonRegressionForm(_lastOrder);
                frm.Show();
            }
        }
        //고정데이터 회귀검사
        private void ChulRegressionToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Application.OpenForms.OfType<ChulsuRegressionForm>().Any())
            {
                var frm = new ChulsuRegressionForm(_lastOrder);
                frm.Show();
            }
        }
        //데이터 마코프검사
        private void MarkovToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Application.OpenForms.OfType<MarkovProcessForm>().Any())
            {
                var frm = new MarkovProcessForm();
                frm.Show();
            }
        }
        //Sql 쿼리검사
        private void QueryDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Application.OpenForms.OfType<QueryFixDataForm>().Any())
            {
                var frm = new QueryFixDataForm();
                frm.Show();
            }
        }
        //데이터 외곽선검사
        private void OutlineToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Application.OpenForms.OfType<OutlineFrameForm>().Any())
            {
                var frm = new OutlineFrameForm(_lastOrder);
                frm.Show();
            }
        }
        //출수 도트변환검사
        private void ChulDigitalToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Application.OpenForms.OfType<ChulsuDisitalForm>().Any())
            {
                var frm = new ChulsuDisitalForm(_lastOrder);
                frm.Show();
            }
        }
        //폴리곤 픽셀검사
        private void PolygonToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Application.OpenForms.OfType<PolygonPixelForm>().Any())
            {
                var frm = new PolygonPixelForm(_lastOrder);
                frm.Show();
            }
        }
        #endregion

        #region FilterMenu
        //정수출수 데이터 필터
        private void KojeonFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Application.OpenForms.OfType<ChulsuFilterForm>().Any())
            {
                var frm = new ChulsuFilterForm(_lastOrder);
                frm.Show();
            }
        }
        //고정출수 데이터 필터
        private void KeykjaFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Application.OpenForms.OfType<FixChulsuFilterForm>().Any())
            {
                var frm = new FixChulsuFilterForm(_lastOrder);
                frm.Show();
            }
        }
        //열별출수 데이터 필터
        private void ChulsuFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Application.OpenForms.OfType<NonChulsuFilterForm>().Any())
            {
                var frm = new NonChulsuFilterForm(_lastOrder);
                frm.Show();
            }
        }
        //합계출수 데이터 필터
        private void HapFilterToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Application.OpenForms.OfType<HapChulsuForm>().Any())
            {
                var frm = new HapChulsuForm(_lastOrder);
                frm.Show();
            }
        }
        //내부상자 데이터 필터
        private void BoxDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Application.OpenForms.OfType<InnerBoxForm>().Any())
            {
                var frm = new InnerBoxForm();
                frm.Show();
            }
        }
        //출수타입 데이터 필터
        private void TypeDataToolStripMenuItem_Click(object sender, EventArgs e)
        {
            if (!Application.OpenForms.OfType<ChulTypeForm>().Any())
            {
                var frm = new ChulTypeForm(_lastOrder);
                frm.Show();
            }
        }
        #endregion

        #region AnalyzeMenu

        #endregion

        #region WinningMenu

        #endregion
    }
}
