function func(me)
    local map = me:map()
    local mobs = me:nears(1)

    for _, mob in pairs(mobs) do
        local base_hp = mob:base_hp()

        -- 현재 체력의 50%만큼의 데미지를 준다.
        local damage = mob:hp() * 0.5

        -- 남은 체력이 10% 미만이라면 즉사
        if mob:hp() - damage < base_hp * 0.1 then
            damage = base_hp
        end

        mob:hp_add(damage)
    end
end