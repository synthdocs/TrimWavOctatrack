using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using NAudio.Wave;

namespace TrimWav
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            input.Text = "c:\\MP3\\import_export_csharp\\import\\essai_24_44100_800renoise_16.wav";
            output.Text = "C:\\MP3\\import_export_csharp\\export\\";
            nbr_partie.Text = "16";
            nom_fichier.Text = "export";
        }

        private void Trim_Click(object sender, EventArgs e)
        {
            // Lecture de l'input
            WaveFileReader reader = new WaveFileReader(input.Text.ToString());
            TimeSpan span = reader.TotalTime;
            long span_total_long = span.Ticks;
            long nbr_partie_long = Int64.Parse(nbr_partie.Text.ToString());
            long variable_saut_wav_long = span_total_long / nbr_partie_long;

            // Variable de saut en TimeSpan
            TimeSpan variable_saut_wav_timespan = TimeSpan.FromTicks(variable_saut_wav_long);

            // boucle de découpe du fichier wav
            for (int i = 0; i < nbr_partie_long; i++)
            {
                string output_string = output.Text.ToString() + i.ToString() + nom_fichier.Text.ToString() + ".wav";
                if (i == nbr_partie_long - 1)
                {
                    WavFileUtils.TrimWavFile(input.Text.ToString(), output_string, TimeSpan.FromTicks(variable_saut_wav_long * i), TimeSpan.FromTicks((variable_saut_wav_long * i) + variable_saut_wav_long));
                    listBox1.Items.Add(i.ToString() + " --- " + TimeSpan.FromTicks(variable_saut_wav_long * i).ToString() + " --- " + TimeSpan.FromTicks((variable_saut_wav_long * i) + variable_saut_wav_long).ToString());
                }
                else
                {
                    WavFileUtils.TrimWavFile(input.Text.ToString(), output_string, TimeSpan.FromTicks(variable_saut_wav_long * i), TimeSpan.FromTicks((variable_saut_wav_long * i) + variable_saut_wav_long - 1));
                    listBox1.Items.Add(i.ToString() + " --- " + TimeSpan.FromTicks(variable_saut_wav_long * i).ToString() + " --- " + TimeSpan.FromTicks((variable_saut_wav_long * i) + variable_saut_wav_long - 1).ToString());
                }
            }
            Form.ActiveForm.Refresh();
        }
    }
}
    public static class WavFileUtils
    {
        public static void TrimWavFile(string inPath, string outPath, TimeSpan cutFromStart, TimeSpan cutFromEnd)
        {
            using (WaveFileReader reader = new WaveFileReader(inPath))
            {
                using (WaveFileWriter writer = new WaveFileWriter(outPath, reader.WaveFormat))
                {
                    int bytesPerMillisecond = reader.WaveFormat.AverageBytesPerSecond / 1000;

                    int startPos = (int)cutFromStart.TotalMilliseconds * bytesPerMillisecond;
                    startPos = startPos - startPos % reader.WaveFormat.BlockAlign;

                    int endBytes = (int)cutFromEnd.TotalMilliseconds * bytesPerMillisecond;
                    endBytes = endBytes - endBytes % reader.WaveFormat.BlockAlign;
                    //int endPos = (int)reader.Length - endBytes;

                    int endPos = endBytes;

                    TrimWavFile(reader, writer, startPos, endPos);
                }
            }
        }

        private static void TrimWavFile(WaveFileReader reader, WaveFileWriter writer, int startPos, int endPos)
        {
            // set blockalign -> 8 pour 24 bit
            Int32 blockalign = reader.WaveFormat.BlockAlign;
            Int32 bytealign = 0;
            // 4 = 1024 pour 16bit, sinon 6 pour 24bit, 384 car c'est divisible par 6 sans faire planter le pc
            if (blockalign == 4)
                bytealign = 1024;
            else
                bytealign = 384;

            reader.Position = startPos;
            byte[] buffer = new byte[bytealign];
            while (reader.Position < endPos)
            {
                int bytesRequired = (int)(endPos - reader.Position);
                if (bytesRequired > 0)
                {
                    int bytesToRead = Math.Min(bytesRequired, buffer.Length);
                    int bytesRead = reader.Read(buffer, 0, bytesToRead);
                    if (bytesRead > 0)
                    {
                        // WriteData obsolète
                        writer.Write(buffer, 0, bytesRead);
                    }
                }
            }
        }
}