using System;
/*
 * *****************************************************************************************************************************************************************
 * MMF tutorial 07 "2D drawing Beginning"
 * 
 * ◎ The purpose of this section
 * Learn how to display a 2D to 3DCG space in 1, MMF
 * 
 * ◎所要時間
 * 20分
 * 
 * ◎難易度
 * 標準
 * 
 * ◎前準備
 * To prepare a project that created the contents of the up-01
 * ★★★ Important ★★★ → x86 are included within the MMFLib in the Sample, x64, Toon, to copy the Shader folder to 
 * the destination folder. Such as bin \\ Debug within.
 * 
 * ◎Step of this tutorial
 * ①～③
 * ・Form1.cs
 * 1 file of only
 * 
 * ◎必須ランタイム
 * DirectX エンドユーザーランタイム
 * SlimDX エンドユーザーランタイム x86 .Net4.0用
 * .Net Framework 4.5
 * 
 * ◎終着点
 * 2Dがいろいろ表示されればOK
 * 
 ********************************************************************************************************************************************************************/

namespace _07_Drawing2DToRenderForm
{
    static class Program
    {
        /// <summary>
        /// Main application エントリ ポイントです。
        /// </summary>
        [STAThread]
        static void Main()
        {
            MMF.MessagePump.Run(new Form1());
        }
    }
}
