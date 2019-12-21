# shiro
It's like if node.js and lisp had a baby, but the mom drank and smoked during pregnancy.  I will eventually getting around to actually writing documentation for this (maybe...), but until then this might help a bit...  Note that I don't promise it's up to date, check out tutorial.txt in the repo for the actual latest version.

    ; Comments are written like this
    ; or you can end a comment early with ;  (nop "nop is a keyword that does nothing")
    
    (print "Hello Nurse!")        ; Baby's first program

    ; Use 'do' to evaluate multiple lists.  The value of the last one will be returned
    (do
        (print "Hello Nurse!")
        (print "This is another line")
    )

    ; String concatenation is pretty easy
    (print (str "Hello " 'Nurse'))    ; You can use either single or double quotes

    (print "Strings can have%s
    line breaks in them to print on multiple lines, and also%s
    can include the 'other kind' of quote in them.  You can escape the%s
    quote you used for the string %"like this%"")


    ;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
    ; Math and comparison uses the usual LISP prefix-notation, like so:
    (+ 2 2)            ; 4
    (+ 2 (- 3 1))      ; 4
    (+ 3 3 3)          ; 9

    (= 2 2)            ; T
    (= 2 2.5)          ; F
    (= 2 (/ 4 2))      ; T
    (= nil "nil")      ; F

    (! true)           ; F
    (! nil)            ; T
    (! 0)              ; T
    (> 3 2)            ; T
    (>= 2 2)           ; T


    ;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
    ; Variables are pretty standard:
    (do
        (def x 1)   ; declare x for the first time and set it to 1.
        (set x 5)   ; set existing variable x to value 5
        (sod y 10)  ; 'sod' makes it easy to work with variables, by using either set or
        (sod y 23)  ; ... define (get it?  s.o.d).  

        ; You need to explicitly get the value of a variable:
        (print (str "x = " (v x)))

        ; ... but there is a reader shortcut to do this using $:
        (print (str "y = " $y))
    )

    ; All variables are global by default, but you can use the keyword 'let' to create a local scope area:
    (do
        (let 
            (x 1 y 2)
            (do
                (print $x) (print $y)))            
        (print (def? x)))        ; F if you haven't defined another global 'x' somewhere

    ; Note that the list passed as the first parameter to 'let' is a rare instance of a list with an implicit 
    ;  quote, simliar to that which inline-objects have.  It always expects an even number of parameters and
    ;  treats the odd numbered ones as names and the even numbered ones as the value's assigned to the
    ;  variables.  It is also worth noting thatlet variables trump global variables, effectively "hiding" 
    ;  them, so if you had a global 'x' defined in the example above, the let-scoped 'x' would still be
    ;  printed inside the let-scope.

    ;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
    ; Lists (code is data yo)
    ;  Note that by default, any list in Shiro (something in parenthesis) will be evaluated, which means that 
    ;  the first item in the list should be a keyword or function name.  So,

    ;(sod x (1 2 3))                                ; will throw an error, because '1' is not a keyword 
                                                    ; or function name.  But you can set a variable to a list 
                                                    ;  without evaluating it by quoting that list:
    (do 
        (sod x (quote 1 2 3))
        ;... and there is a reader shortcut for this, with single-quoting your list:
        (sod x '(1 2 3))

        ; You can do some pretty cool things with quoted lists, like:
        (eval (concat '(print 0) $x))            ; Concatenating a new command-list and evaluating it
        (eval (skw print $x))                    ; skw sets the 'keyword' (first item) in the list

        (sod x '(1 2 "dan was here" "hello world"))
        (print (filter str? $x))                            ; Pull out the strings.  The first parameter to 
                                                            ;  filter can be any keyword or function.

        (defn say-hi (name) (print (str "Hello " $name)))    ; By the way this is how you declare functions
        (map say-hi '("Dan" "Dhiraj" "Dave"))       ; map evaluates the first parameter individually against
                                                    ;  everything in the list in the second parameter and 
                                                    ;  returns the result of that evaluation
        ;the line above is equivalent to:
        (say-hi "Dan")
        (say-hi "Dhiraj")
        (say-hi "Dave")

        ;You can also map to keywords, like this:
        (map print '(1 2 3))
    )

    ; There are lots of ways to slice-and-dice lists:
    (1st '(1 2 3))           ; 1
    (rest '(1 2 3))          ; 2 3 
    (nth 2 '(1 2 3))         ; 2
    (range 2 2 '(1 2 3 4))   ; 2 3


    ;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
    ; Remember that 'str?' thing from the filter example above?  Let's look at some of the other ones:
    (sod x '(1 2 3))
    (sod y 2)
    (sod z {name: "dan", age: 35})       ; More on this later, don't panic!
    (sod s "Hello nurse")

    (list? $x) (list? $z)                ; T
    (list? $y) (list? $s)                ; F

    (obj? $z)                            ; T
    (obj? $x) (obj? $y) (obj? $s)        ; F

    (num? $y)                            ; T
    (num? $x) (num? $z) (num? $s)        ; F

    (def? x) (def? s)                    ; T
    (def? bob)                           ; F


    ;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
    ; Now that we know a bunch of things that evaluate to booleans, let's figure out how to branch,
    ;  Shiro provides a very innovative and bleeding-edge keyword for this, called 'if':

    (if true (print "Hello world"))
    (if false (print "Won't Print") (print "Will Print"))

    ; if acts like a ternary operator (? :) as well:
    (if true 1 2)        ; 1
    (if false 1 2)        ; 2
    (if 0 1)            ; nil

    ; and of course we can loop (actually there are lots of interesting ways to look, like map, apply, 
    ;  and filter, but this is the boring way)
    (do
        (sod x 10)
        (while (> $x 0) (do
            (print $x)
            (set x (- $x 1)))))


    ;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
    ; WTF are those curly-braces doing in my Lisp-like?
    ;  Well, they're called 'inline-objects' and they help make the stuff Shiro
    ;  was designed to do easier.  They are just lists, but lists that:
    ;        a)  Are not evaluated like normal lists (they have an 'implicit quote')
    ;        b)  Where each element in the list has a name as well as a value
    
    (do
        (sod x {name: "Dan", o: {f: 1}})     ; if you treated this as a list, it's:  '("Dan" '(1))

        (. $x name)                          ; x.name in a more boring programming language
        (. $x o f)                           ; ditto:  $x.o.f

        (.s x name 'Peter')                  ; x.name = 'Peter' in a more boring language

        (sod s (json $x))                    ; json returns the inline-object in question, JSON-serialized.
        (sod obj (dejson $s))                ; duh

        (print (. $obj name))                ; obj is now the same as x, by way of JSON

        ;(. $obj fakeProperty)               ; This would throw an error because fakeProperty does not exist
        (.? $obj fakeProperty))              ; This does not throw an error, and returns nil.

        ; Pair creates an item with a name.  Because inline-objects are just lists, we can then concatenate
        ;   that onto a list, effectively injecting a property:
        (sod x (concat $x (pair fakeProperty "Its magic!")))    
        (print (.? $x fakeProperty)))        ; "It's magic!"
    )


    ;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
    ; Lambdas and Tigers and bears (oh my!)

    ;The most obscure way to calculate 2+2 in the world:
    (=> (x y) (+ $x $y)) 2 2
    
    (map (=> (s) (print $s)) '("Dan" "Dhiraj" "Dave"))

    (=> (s) (print $s)) 'hello world'

    (filter (=> (n) (> $n 5)) '(1 10 7 3 -4 154))

    ; Notice the difference between:
    (apply (=> (x) (+ $x 1)) '(1 2 3 4))        ; results in '(1 2 3 4)
    (map (=> (x) (+ $x 1)) '(1 2 3 4))          ; results in `(2 3 4 5)

    ;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
    ; Sometimes you have a thought in the shower like, "lisp would make a way better node.js than js"
    ;  and sometimes you actually go and do it.  But before we get into that, let's talk about
    ;  the world's simplest telnet chat server:
    
    (telnet 4676 (sendAll (str $id " says '" $input "'")))

    ; Shiro has a few keywords (like 'telnet' above) that go into network server mode.  This has a few 
	; properties:
    ;  a)  The interpreter's main thread (the one that executes your Shiro) begins blocking.  A 
    ;       multi-threaded network server component will begin listening, and as events occur which 
    ;       evaluate Shiro they will be evaluated by the network server's threads.  Don't worry, your 
    ;       Shiro is always thread-safe.
	;
    ;  b)  The main thread doesn't go away -- all your code and variables are still there, and if 
    ;       the network-server ever executes a 'stop' keyword it will come right back.  You can even 
    ;       return something from the network thread to the main thread by passing it as a parameter to
	;       stop.  Here's a telnet server that can be stopped:

    (telnet 4676 
        (if (= $input "quit") 
            (do (print "quitting")(stop $input))
            (print $input)))
            
    ;      If you telnet into this server and type anything it will print out in the Shiro window.  If you 
    ;       send it 'quit' the Shiro server will stop listening and return "quit" to the main thread.
    ;
    ;  c)  A series of local variables (ie: 'let' scoped variables) will be created for Shiro evaluated in 
    ;       the server's context.  For telnet, these are id (a guid-as-string uniquely identifying the socket 
    ;       which triggered the evaluation) and input (the full line-command sent to the server).
    ;
    ;  d)  Several different keywords will become available for use, depending on the type of the server.  
    ;       In this telnet example, they are send, sendTo and sendAll.

    ; Telnet can also take an optional third parameter which is a list that will be evaluated whenever 
    ;  someone connects, this has an id let-scoped variable, but not an input:

    (telnet 4676 
        (send $input)                    ; echo whatever is sent back to the client
        (send "Hello and welcome!"))
    

    ;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;;
    ; Okay, we're finally ready for the fun stuff!  Let's make some web services:

    (http 8676 
        (print (. $request url))

    ; This is pretty lame.  It listens on port 8676, negotiates an HTTP connection, prints out
    ;  the request url and then returns the request url back to the client as whatever the default
    ;  content-type is.  It's worth noting that there are no $id and $input like in a telnet server,
    ;  instead there is a $request variable, which is an inline object that has the HTTP request properties
	;  on it

    ; Today's web services are normally returning json, so let's do that:
    
    (http 8676 
        (content "application/json" 
            (json {name: "Dan Larsen", age: 35})))        

    ; content is a keyword which can only be executed in an http-server context (like telnet's 
    ;  'send' variants) it sets the content-type header of the returned value.  We can use the json keyword 
    ;  that you already know to turn one of our inline-objects into a json string.

    ; So normally websites and webservices have different endpoints on them, they don't respond to all 
    ;  connections the same way.  We call this routing the request, and like most of this stuff so far, it's
    ;   pretty easy to do in Shiro:

    (http 8676 (route
        "getJson" (content "application/json" (json {name: "Dan", age: 35}))
        "quit" (stop)
        "default" (status 404 "Endpoint not Found")))

    ; Check out:
    ;   http://localhost:8676/getJson
    ;   http://localhost:8676/pageDoesntExist
    ;   http://localhost:8676/quit

    ; Alright let's put it all together and build a mocked web service which can store and retrieve data:

    (do 
        (sod data {dan: {name: "Dan", age: 35}, dhiraj: {name: "Dhiraj", age: 28}})
        (defn getOrDefault (key) 
            (if (nil? (.? $data $key)) 
                (status 404 "Id not found") 
                (. $data $key)))
        (http 8676 (route
            "get"   (content "application/json" (json (getOrDefault (. $request args id))))
            "store" 
                (do
                    (set data (concat $data (pair (. $request args id) (dejson (. $request args json)))))
                    (status 200 "Saved"))
            "quit" (stop $data))))

    ; Check out:
    ;   http://localhost:8676/get?id=dhiraj
    ;   http://localhost:8676/get?id=1        Not found
    ;   http://localhost:8676/store?id=1&json={name:"Steve",age:21}
    ;   http://localhost:8676/get?id=1

