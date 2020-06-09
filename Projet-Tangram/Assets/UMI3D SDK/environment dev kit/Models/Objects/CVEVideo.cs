/*
Copyright 2019 Gfi Informatique

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

    http://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/
using System;
using System.Collections;
using System.Collections.Generic;
using umi3d.common;
using UnityEngine;
using UnityEngine.Video;

namespace umi3d.edk
{
    /// <summary>
    /// Component Overidding a genericObject3D texture to display a viedo on it
    /// Require a genericObject3D on the same GameObject;
    /// </summary>
    [RequireComponent(typeof(AbstractObject3D))]
    public class CVEVideo : MonoBehaviour, IHasAsyncProperties
    {
        [SerializeField]
        bool Button_Play;
        [SerializeField]
        bool Button_Pause;
        [SerializeField]
        bool Button_Stop;


        protected List<IHasAsyncProperties> lstListeners = new List<IHasAsyncProperties>();
        protected bool inited = true;
        protected bool updated = false;
        protected List<UMI3DUser> updatedFor = new List<UMI3DUser>();

        AbstractObject3D _object3D;
        /// <summary>
        /// The genericObject this component is attached to
        /// </summary>
        public AbstractObject3D Object3D
        {
            get { if (!_object3D) _object3D = GetComponent<AbstractObject3D>(); return _object3D; }
            set { _object3D = value; }
        }

        double startdate = 0;
        double pausedate = 0;

        private string LastVideoUrl = null;
        /// <summary>
        /// Resource path of the video
        /// </summary>
        public CVEResource VideoResource;

        /// <summary>
        /// Does the video play when it's ready
        /// </summary>
        public bool PlayOnAwake = false;
        /// <summary>
        /// Does the video restart when it's over
        /// </summary>
        public bool Loop = false;

        /// <summary>
        /// Sound volume video, between 0 and 1.
        /// </summary>
        [Range(0, 1)]
        public float Volume = 0.6f;

        /// <summary>
        /// Switch between Progress mode or start date mode to synchronise video
        /// </summary>
        public bool useProgressMode = false;

        private double Progress;

        private bool Playing = false;
        private bool Paused = false;

        public UMI3DAsyncProperty<bool> objectPlayOnAwake;
        public UMI3DAsyncProperty<bool> objectLoop;

        public CVEAudioSource AudioSource;
        private VideoPlayer VideoPlayer;

        public void initDefinition()
        {
            objectPlayOnAwake = new UMI3DAsyncProperty<bool>(this, PlayOnAwake);
            objectPlayOnAwake.OnValueChanged += (bool value) => PlayOnAwake = value;

            objectLoop = new UMI3DAsyncProperty<bool>(this, Loop);
            objectLoop.OnValueChanged += (bool value) => Loop = value;

            VideoResource.initDefinition();
            VideoResource.addListener(this);

            if (AudioSource == null)
            {
                GameObject g = new GameObject();
                g.transform.SetParent(transform);
                AudioSource = g.AddComponent<CVEAudioSource>();
            }

            VideoPlayer = gameObject.GetComponentInChildren<VideoPlayer>(true) ?? (Object3D.preview ? Object3D.preview.AddComponent<VideoPlayer>() : null);
            VideoPlayer.playOnAwake = false;
            VideoPlayer.Pause();
            if (AudioSource)
            {
                StartCoroutine(setAudioSource());
            }

            SyncProperties();

            Playing = false;
            Paused = false;

            pausedate = 0;
            startdate = 0;

            if (PlayOnAwake)
                Play();

        }

        IEnumerator setAudioSource()
        {
            if (AudioSource)
            {
                while (!AudioSource.gameObject.GetComponent<AudioSource>())
                {
                    yield return 0;
                }
                VideoPlayer.SetTargetAudioSource(0, AudioSource.gameObject.GetComponent<AudioSource>());
            }
            yield return 0;
        }

        public void Play()
        {
            if (!Playing)
            {

                bool lastPaused = Paused;
                Playing = true;
                Paused = false;
                NotifyUpdate();
                SyncProperties();

                if (lastPaused != Paused && pausedate != 0)
                {
                    VideoPlayer.Play();
                    startdate = startdate + (getTimeInMilliseconds() - pausedate);
                }
                else
                {
                    VideoPlayer.Play();
                    startdate = getTimeInMilliseconds();
                }
            }
        }

        public void Pause()
        {
            if (Playing)
            {
                pausedate = getTimeInMilliseconds();
                Playing = false;
                Paused = true;
                NotifyUpdate();
                SyncProperties();


                VideoPlayer.Pause();
            }
        }

        public void TogglePlay()
        {
            if (Playing)
            {
                Pause();
            }
            else
            {
                Play();
            }
        }

        public void Stop()
        {
            if (Playing || Paused)
            {
                startdate = 0;
                pausedate = 0;
                Playing = false;
                Paused = false;
                NotifyUpdate();
                SyncProperties();
                VideoPlayer.Pause();
                VideoPlayer.frame = 0;
            }
        }

        public VideoDto ToDto(UMI3DUser user)
        {

            var dto = new VideoDto();
            dto.VideoResource = VideoResource.ToDto(user);
            dto.PlayOnAwake = objectPlayOnAwake.GetValue(user);
            dto.Loop = objectLoop.GetValue(user);
            dto.Volume = Volume;

            dto.AudioSource = AudioSource.Id;

            dto.Playing = Playing;
            dto.Paused = Paused;

            if (useProgressMode)
            {
                dto.Progress = (VideoPlayer.frameCount > 0) ? (VideoPlayer.frame / (double)VideoPlayer.frameCount) : 0;
                dto.StartTimeInMS = string.Empty;
            }
            else
                dto.StartTimeInMS = startdate.ToString();

            return dto;
        }

        protected void SyncProperties()
        {
            if (inited)
            {
                VideoResource.SyncProperties();
                SyncVideoProp();
                SyncVideo();
            }
        }

        void SyncVideoProp()
        {
            objectPlayOnAwake.SetValue(PlayOnAwake);
            objectLoop.SetValue(Loop);
            NotifyUpdate();
        }

        public void SyncVideo()
        {
            if (VideoPlayer != null)
            {
                VideoPlayer.isLooping = Loop;

                if (LastVideoUrl != VideoResource.GetUrl())
                {
                    SyncVideoClip();
                    LastVideoUrl = VideoResource.GetUrl();
                }
            }
        }

        public void SyncVideoClip()
        {

            if (VideoResource.GetUrl() != null && VideoResource.GetUrl().Length > 0)
                StartCoroutine(LoadClip(VideoResource));
            else
                VideoPlayer.url = null;
        }

        IEnumerator LoadClip(CVEResource resource)
        {
            VideoPlayer.Stop();
            VideoPlayer.url = resource.GetUrl();
            VideoPlayer.Prepare();
            while (!VideoPlayer.isPrepared)
            {
                yield return 0; // code will wait till file is completely read
            }
            if (PlayOnAwake && Playing)
            {
                VideoPlayer.Play();
                NotifyUpdate();
            }
            Debug.Log(VideoPlayer.frameCount);
            yield return 0;
        }

        double getTimeInMilliseconds()
        {
            return (DateTime.Now - DateTime.MinValue).TotalMilliseconds;
        }

        public void NotifyUpdate()
        {
            updated = true;
            foreach (var listener in lstListeners)
            {
                listener.NotifyUpdate();
            }
        }

        public void NotifyUpdate(UMI3DUser u)
        {
            if (!updatedFor.Contains(u))
                updatedFor.Add(u);

            foreach (var listener in lstListeners)
            {
                listener.NotifyUpdate(u);
            }
        }

        public void addListener(IHasAsyncProperties listener)
        {
            if (!lstListeners.Contains(listener))
                lstListeners.Add(listener);
        }

        public void removeListener(IHasAsyncProperties listener)
        {
            if (lstListeners.Contains(listener))
                lstListeners.Remove(listener);
        }


        private void Update()
        {
            if (Button_Play)
            {
                Play();
                Button_Play = false;
            }
            if (Button_Stop)
            {
                Stop();
                Button_Stop = false;
            }
            if (Button_Pause)
            {
                Pause();
                Button_Pause = false;
            }
        }

    }

}
