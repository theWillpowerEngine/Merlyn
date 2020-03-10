# shiro
Shiro is a programming language that looks a lot like LISP -- in fact you could be forgiven for thinking it's just a LISP dialect, until you start writing it and notice it also has JavaScript objects in it and breaks a few of the rules that LISP purists take for granted.  Oh, and all the keywords are different.

Shiro is part object oriented, part functional, all dynamic and intended to develop little useful things very quickly.  I use it a lot to mock services for web development, to write small DevOps tools and to create low yield web services.  It also has a built in Telnet server to go with the HTTP server if you feel like building a MUD or something.

There is a (work-in-progress) guide in the repo at resources/How to Shiro.pdf.  

If you're curious as to why Shiro is worth learning at all, well...  I challenge you to write a fully functional REST service that you can call with GET, PUT, POST, DELETE and PATCH implemented in less code than this:

	(do 
		(def data '({id: 1, name: "Dan", age: 35}
			   {id: 2, name: "Dhiraj", age: 28}))
		(http 8676 (route
			 "api*" (rest $data id))))

I'm working on the guide now so this will all make sense to the 0-1 of you who will ever care shortly.
