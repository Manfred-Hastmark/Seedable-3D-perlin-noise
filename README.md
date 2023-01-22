# Seedable 3D Perlin noise

3D Perlin noise algoritm based on algoritm described in article [Simplex noise demystified](https://weber.itn.liu.se/~stegu/simplexnoise/simplexnoise.pdf)

![](https://github.com/Manfred-Hastmark/Seedable-3D-perlin-noise/blob/main/example.JPG?raw=true)
Example of what the noise can look like

## Usage

Create an instance of Perlin3D with a seed, here for example, the seed 123456789 is used

```csharp
Perlin3D perlin = new Perlin3D(123456789);
```

Instance the Perlin noise by inserting a point like below

```csharp
float x,y,z;

float evalPerlin = perlin.Noise(x, y, z);

```
## Contributing

Pull requests are welcome. For major changes, please open an issue first
to discuss what you would like to change.

## License

[MIT](https://choosealicense.com/licenses/mit/)
