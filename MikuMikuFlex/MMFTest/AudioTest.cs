/*
* Copyright (c) 2007-2009 SlimDX Group
* 
* Permission is hereby granted, free of charge, to any person obtaining a copy
* of this software and associated documentation files (the "Software"), to deal
* in the Software without restriction, including without limitation the rights
* to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
* copies of the Software, and to permit persons to whom the Software is
* furnished to do so, subject to the following conditions:
* 
* The above copyright notice and this permission notice shall be included in
* all copies or substantial portions of the Software.
* 
* THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
* IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
* FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
* AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
* LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
* OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
* THE SOFTWARE.
*/

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using SlimDX;
using SlimDX.XAudio2;
using SlimDX.Multimedia;
using WaveStream = SlimDX.Multimedia.WaveStream;
using NAudio;
using NAudio.Wave;

namespace BasicSound
{
    class AudioPlayer
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]	//
        static extern short GetAsyncKeyState(int key);		//spedific to this example. see below
        const int VK_ESCAPE = 0x1B;							//

        public static void AudioMain()
        {
            XAudio2 device = new XAudio2(); //audio output stream/device

            MasteringVoice masteringVoice = new MasteringVoice(device); //no idea, presumably prepares the out stream

            // play a PCM file
            PlayPcm(device, "MusicMono.wav"); //this actually plays a wav file

            // play a 5.1 PCM wave extensible file
            PlayPcm(device, "MusicSurround.wav");

            masteringVoice.Dispose();	//cleanup
            device.Dispose();			//
        }

        public static void PlayPcm(XAudio2 device, string fileName)
        {
            var s = System.IO.File.OpenRead(fileName);	//open the wav file
            WaveStream stream = new WaveStream(s);		//pass the stream to the library
            s.Close();	//close the file

            AudioBuffer buffer = new AudioBuffer();	//init the buffer
            buffer.AudioData = stream;				//set the input stream for the audio
            buffer.AudioBytes = (int)stream.Length;	//set the size of the buffer to the size of the stream
            buffer.Flags = BufferFlags.EndOfStream; //presumably set it to play until the end of the stream/file 


            SourceVoice sourceVoice = new SourceVoice(device, stream.Format); //this looks like it might initalise the actual output
            sourceVoice.SubmitSourceBuffer(buffer); //pass the buffer to the output thingo
            sourceVoice.Start(); //start the playback?

            //above 2 sections are guessed, there is no documentation on the classes/proerties.

            // loop until the sound is done playing
            while (sourceVoice.State.BuffersQueued > 0)	// This keeps looping while there is sound in the buffer
            {											// (presumably). For this specific example it will stop
                if (GetAsyncKeyState(VK_ESCAPE) != 0)	// plying the sound if escape is pressed. That is what the
                    break;								// DLLImport and stuff at the top is for
                Thread.Sleep(10);						//
            }

            // wait until the escape key is released
            while (GetAsyncKeyState(VK_ESCAPE) != 0) //it jsut waits here until the person presses escape
                Thread.Sleep(10);

            // cleanup the voice
            buffer.Dispose();
            sourceVoice.Dispose();
            stream.Dispose();
        }

        static WaveOutEvent waveOut;
        static Mp3FileReader mp3Reader ;

        public static void PlayMp3Async(string filename)
        {
            Task.Factory.StartNew((s) => PlayMp3((string)s), filename);
        }

        public static void PlayMp3(string filename)
        {
            waveOut = new WaveOutEvent();
            mp3Reader = new Mp3FileReader(filename);
            waveOut.Init(mp3Reader);
            waveOut.Play();
            
            // reposition to five seconds in
            mp3Reader.CurrentTime = TimeSpan.FromSeconds(5.0);
            //waveOut.PlaybackStopped += OnPlaybackStopped;
            //waveOut.Stop();
            Console.ReadLine();

            while (waveOut.PlaybackState == PlaybackState.Playing)
            {
                Thread.Sleep(1000);
            }
            mp3Reader.Dispose();
            waveOut.Dispose();
        }

        private static void OnPlaybackStopped(object sender, StoppedEventArgs e)
        {

        }
    }
}