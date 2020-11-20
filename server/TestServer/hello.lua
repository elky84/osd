function func(c)
    print(string.format('hp : %s', c:hp()))
    print(string.format('damage : %s', c:damage()))

    local result = c:yield()
    print(string.format('return value : %s', result))

    print(string.format('hp : %s', c:hp()))
    print(string.format('damage : %s', c:damage()))
end