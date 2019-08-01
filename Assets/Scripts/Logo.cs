using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
public class Logo : MonoBehaviour


    
    {

        private string movie = "Saffellian games_4";

        void Start()
        {
            StartCoroutine(StreamVideo(movie));
        }

        private IEnumerator StreamVideo(string video)
        {
            Handheld.PlayFullScreenMovie(video, Color.black, FullScreenMovieControlMode.Hidden, FullScreenMovieScalingMode.Fill);
            yield return new WaitForEndOfFrame();
            SceneManager.LoadScene("Menu");
        }
    }
