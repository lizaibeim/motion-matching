# MotionMatching

## Introduction
The Motion Matching, a next-gen animation technology, is a novel method for generating high quality, smooth and complicated character movement. Rather than playing specific artificial animations, Motion Matching makes the characters in game more like performing some real-life actions which are recorded by motion capturing in reality. Motion Matching allows animator to specific the significant characteristics of animation clips which should be emphasized, and the best matching clip is selected based on the nearest neighbor search when doing the comparison among the motion capture database. Compared to the traditional animation system, Motion Matching demonstrates a simple and effective approach to modify the current animation system by directly generating new motion database. Since it is a data-driven method, to a certain extent the performance of the Motion Matching system is positively correlated with the size of the motion capture database. Apart from that, the performance of the system are bound by the CPU as well as the memory because it does the real-time computation consecutively by a very short time interval while on running. This results in the trade off among the responsiveness of the system, the accuracy of the system and the budgets on CPU, memory.  

The main objective of the project is to implement the basic functionality of a working Motion Matching system and integrate it into the game [The Bleeding Tree](https://dadiu.itch.io/the-bleeding-tree). The objectives consist of two parts; the first part is in data pre-processing phase building up the motion capture database; and the second part is in run-time phase finding out the best matching clip to play next. During the process of implementing the system, Translate-Rotate-Scale(TRS) affine matrix is used to convert the coordinates, K Nearest Neighbour(KNN) algorithm is used to search the best matching animation clip, and Principal Component Analysis(PCA) method is used to reduce the dimensions of comparing feature.

## Demo
This video shows the basic locomotion with using the motion matching system. The left subtitle is the real-time matching results. The right part is the real-time controller’s input. There are two lines in the demonstration. The line which is straighter, and smoother is the predicted trajectory, and the other line is the matched trajectory. Both of these two lines are relative to the characters’ current position and rotation

https://user-images.githubusercontent.com/38242437/176313086-a0fcd9f1-1c2c-4207-9c83-0a30c4fc5ca0.mp4

This video is to show that the system can simulate the deceleration process

https://user-images.githubusercontent.com/38242437/176312105-bef695dd-490e-4cae-a9b2-24f01b4e7de6.mp4

## Report
[MotionMatching.pdf](https://github.com/lizaibeim/motion-matching/files/9305196/MotionMatching.pdf)

## Aurthor
+ [Zaibei Li](https://www.linkedin.com/in/zaibei-eric-li/)

## Acknowledgement
https://www.gameanim.com/2016/05/03/motion-matching-ubisofts-honor/  
http://www.ipab.inf.ed.ac.uk/cgvu/deeplearningmotion.html  
http://grail.cs.washington.edu/projects/motion-fields/  
https://research.cs.wisc.edu/graphics/Gallery/kovar.vol/MoGraphs/
