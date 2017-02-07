var NewsBox = React.createClass({
    getInitialState: function () {
        return { data: [] };
    },

    loadFromServer: function () {
        
        $.get(this.props.url, function (data) {
            this.setState({ data: data });
        }.bind(this));
    },


    componentWillMount: function () {
        this.loadFromServer();
        window.setInterval(this.loadFromServer, this.props.pollInterval);
    },


    render: function () {
        return (
        <div className="newsBox">
            <NewsList data={this.state.data} />
        </div>
       );
    }
});

var NewsList = React.createClass({
    render: function () {
        var newsNodes = this.props.data.map(function (newsItem) {
            return (
                <NewsItem imgSrc={newsItem.UserPicturePath} uploaderID={newsItem.UserID} uploader={newsItem.UserName} key={newsItem.Id}>
                    {newsItem.News1}
                </NewsItem>
            );
        });
        return (
        <div className="newsList">
            {newsNodes}
        </div>
        );
    }
});


var NewsItem = React.createClass({
    render: function () {
        var md = new Remarkable();
        return (
        <div className="newsItem">
            <a href={'../User/Profile/'+this.props.uploaderID.toString()}>
            <img className="uploaderImage" src={this.props.imgSrc.toString()} />
            <span className="uploaderProfile">
                {this.props.uploader.toString()}
            </span>
            <br />
            <div className="newsSection">
                {this.props.children.toString()}
            </div>
            </a>
        </div>
      );
    }
});


ReactDOM.render(
<NewsBox url="/Home/getNews" pollInterval="150000" />,
  document.getElementById('newsContainer')
);
