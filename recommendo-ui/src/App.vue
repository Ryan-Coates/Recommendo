<template>
  <div id="app">
    <div id="nav">
    <router-link to="/">Home</router-link> |
    <router-link to="/about">About</router-link>
    </div>
    <b-navbar type="dark" variant="redommendo" align="center">
    <b-navbar-nav>
      <router-link class="nav-link" to="/movies">Movies</router-link>
      <router-link class="nav-link" to="/series">Series</router-link>
      <router-link class="nav-link" to="/games">Games</router-link>
      <router-link class="nav-link" to="/books">Books</router-link>
      <router-link class="nav-link" to="/music">Music</router-link>
    </b-navbar-nav>
  </b-navbar>
   <b-navbar type="dark" variant="redommendo" align="center">
    <b-navbar-nav>
      <router-link class="nav-link" to="/recipes">Recipes</router-link>
      <router-link class="nav-link" to="/restaurants">Restaurants</router-link>
      <router-link class="nav-link" to="/daysout">DaysOut</router-link>
      <router-link class="nav-link" to="/kidsactivities">KidsActivities</router-link>
    </b-navbar-nav>
  </b-navbar>
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
.bg-redommendo
{
  background-color: #9a2121;
  width: 100%;
}
.nav-link{
  color: black !important;
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
