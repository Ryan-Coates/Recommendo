<template>
  <div id="app">
    <router-link to="/login">Login</router-link>
    <div id="nav">
      <router-link to="/">Home</router-link> |
      <router-link to="/about">About</router-link> |
      <router-link to="/movies">Movies</router-link> |
      <router-link to="/series">Series</router-link> |
      <router-link to="/games">Games</router-link> |
      <router-link to="/books">Books</router-link> |
      <router-link to="/music">Music</router-link> |
      <router-link to="/recipes">Recipes</router-link> |
      <router-link to="/restaurants">Restaurants</router-link> |
      <router-link to="/daysout">DaysOut</router-link> |
      <router-link to="/kidsactivities">KidsActivities</router-link>
    </div>
    <router-view/>
  </div>
</template>

<style lang="less">
#app {
  font-family: Avenir, Helvetica, Arial, sans-serif;
  -webkit-font-smoothing: antialiased;
  -moz-osx-font-smoothing: grayscale;
  text-align: center;
  color: #2c3e50;
}

#nav {
  padding: 30px;

  a {
    font-weight: bold;
    color: #2c3e50;

    &.router-link-exact-active {
      color: #42b983;
    }
  }
}
</style>
