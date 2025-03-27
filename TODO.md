# Project

Project Description

<em>[TODO.md spec & Kanban Board](https://bit.ly/3fCwKfM)</em>

### Todo

- [ ] ao tentar conectar com zona tia pode ser q a filha da tia q devia se conectar n se conected e as duas zonas acabem sem uma forma de se ligar. Ideal seria então não seria calcular o peso da tia mas sim da filha, no caso não havendo filha spawnada usa o peso da tia  
- [ ] assegurar que peso 0 não está sendo sorteado  
- [ ] Resolver area ratio subzona. Teoricamente a zona mae tem q ter a soma das filhas  
- [ ] metodo de Bake para zona. Criar as bordas e move oque for lista para array  
- [ ] talvez na função de crescimento em L crescer nas outras bordas tambem, ou ter forma de crescer de crescer em ambas, hora na borda L, hora nas regurales denpendedo da maior  
- [ ] na zona add: expand to largest side, rect  
- [ ] try expand privadas e public try expand L e try expand rect  
- [ ] add check spacek nas try expand  
- [ ] mudar is full line pq parece ser a borda completa as pode ser a linha do L que n é uma borda completa  
- [ ] trocar space por distance  
- [ ] retangular, para quando chegar no tamanho maximo definido  
- [ ] possivel problema ao retorna a linha na checagem de distancia, a linha é um novo objecto e n corresponde a referencia das bordas. Talvez mudar a linha de resultado como a linha que esta livre  
- [ ] add restrição de minimo de celulas par forma L, pode ser na classe do metodo  
- [ ] plot inicial em subzona muito pequena pode falhar por falta de espaço  
- [ ] Mover "AssignCellToZone" para floorPlanManager  
- [ ] talvez o ideal é a classe da zona não modificar a grid diretamente. Adicionar metor de verificação para crescer e de acesso as variaveis para q o metodo faça as mudanças. Talvez essa modificação pode ficar no floor plan manager  
- [ ] criqar namespace  
- [ ] trocar delegates por actions, ou não  

### In Progress


### Done ✓

- [x] classe de utils e random. Pode ser a mesma  
- [x] crescer para onde tiver mais espaço ao inves de aleatorio  
- [x] MUDAR O ARMAZENAMENTO DAS BORDAS PARA CLASSE  

