def factorial (number):
    res=1
    for i in xrange(2, number+1):
        res*=i
    return res