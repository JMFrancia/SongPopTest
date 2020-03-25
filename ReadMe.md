# Songpop Test

Design & Code by Joe Francia, for Gameloft Project Manager application, March 2020
---
### Notes
- I wound up going with a classroom motif for the game's aesthetics. Don't ask me why. Maybe it was the mentality of taking a test.

- Although the data JSON provided includes images for each song clip, I chose NOT to use them. Reason is that since media requests being sent out are unencrypted, a sneaky player could intercept those calls using a web debugging proxy, view the pictures, and use them to answer questions. Best workaround would be to use public or private key server-side encryption, but obviously that's out of scope for this test.

- When initially designing this game I thought it made most sense to split it into three scenes (Title, Game, Results). Toward the end of the project I chose to have all scenes share some identical UI (background, blur effect). In retrospect, would have rather considered a different solution to keep the shared UI consistent, either making the whole game in one scene or managing a persistent camera and canvas. Ultimately decided to just leave as is given scope of the assignment, but I wanted to let you know I'm aware it's not best practice.

- Thank you very much for the opportunity! This was a really fun project.
